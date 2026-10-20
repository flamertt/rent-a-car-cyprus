using Microsoft.Extensions.Options;
using RentACar.Modules.Branches.Contracts.Dtos;
using RentACar.Modules.Branches.Contracts.Enums;
using RentACar.Modules.Fleet.Contracts;
using RentACar.Modules.Fleet.Contracts.Enums;
using RentACar.Modules.Reservations.Contracts;
using RentACar.Modules.Reservations.Contracts.Enums;
using RentACar.Modules.Simulation.Application.Options;
using RentACar.Modules.Simulation.Domain.Entities;
using RentACar.Modules.Simulation.Domain.Enums;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Simulation.Application.Engine;

/// <summary>
/// Asgari hareket (güçlü filo varsayımı): haritada her an en az <see cref="SimulationOptions.MinMovingVehicles"/> araç
/// yolda olsun. Eksik varsa önce park hâlindeki müşteriler erken yola çıkar (gece: kasabaya eğlenceye), yetmezse
/// operasyon ekibi boştaki araçları şubeler arasında transfer eder (gece vardiyası sabah uçuşları için Ercan'a ağırlık
/// verir). Kalkışlar birkaç dakikaya yayılır; hızlı ileri sarma (yetişme) sırasında çağrılmaz.
/// </summary>
internal sealed class ActivityFloor(IReservationsApi reservations, IFleetApi fleet, IOptions<SimulationOptions> options, EventLog log)
{
    /// <summary>Bir operasyon karesinde en çok bu kadar yeni kalkış: araçlar aynı anda yola çıkıp aynı anda varmasın.</summary>
    private const int MaxStartsPerRun = 6;

    /// <summary>Bu kadar içinde kalkacak bacak "yolda" sayılır (yeni kalkışlar 1–8 dk sonrasına planlanır).</summary>
    private static readonly TimeSpan Imminent = TimeSpan.FromMinutes(10);

    /// <summary>İadesine bundan az kalan müşteri ek geziye çıkarılmaz.</summary>
    private static readonly TimeSpan ReturnMargin = TimeSpan.FromHours(4);

    /// <summary>Teslimi bu kadar içinde olan araç transfere çıkarılmaz.</summary>
    private static readonly TimeSpan PickupHorizon = TimeSpan.FromHours(12);

    public static int CountMoving(IEnumerable<Trip> trips, DateTimeOffset now) =>
        trips.Count(t => t.State == TripState.Driving || (t.State == TripState.Waiting && t.DepartAt <= now + Imminent));

    public async Task RunAsync(SimContext ctx, List<Trip> trips, Action<Trip> addTrip, CancellationToken ct)
    {
        var deficit = Math.Min(options.Value.MinMovingVehicles - CountMoving(trips, ctx.Now), MaxStartsPerRun);
        if (deficit <= 0)
        {
            return;
        }

        // Gece (00–07) dışarıda olan müşteri az; açığın çoğunu operasyon kapatır.
        var night = KktcTime.ToLocal(ctx.Now).Hour < 7;
        var started = SendCustomersOut(ctx, trips, night ? (deficit + 2) / 3 : deficit, night);
        if (started < deficit)
        {
            await TransferIdleAsync(ctx, trips, addTrip, deficit - started, night, ct);
        }
    }

    /// <summary>
    /// Park hâlindeki müşteri bacağını öne çeker: gündüz planlı bir sonraki hareket hemen başlar; gece konaklamadaki
    /// müşteri en yakın kasabaya eğlenceye çıkar. Dönüş ve iade yine <see cref="ItineraryPlanner"/>'a kalır.
    /// </summary>
    internal static int SendCustomersOut(SimContext ctx, IEnumerable<Trip> trips, int count, bool night)
    {
        if (count <= 0)
        {
            return 0;
        }

        var chosen = trips
            .Where(t => t is { Kind: TripKind.Rental, State: TripState.Waiting, Purpose: LegPurpose.Excursion or LegPurpose.BackToStay }
                && t.DepartAt > ctx.Now + Imminent
                && t.ReturnAt - ctx.Now > ReturnMargin)
            .OrderBy(_ => ctx.Rng.Next())
            .Take(count)
            .ToList();
        foreach (var trip in chosen)
        {
            var departAt = ctx.Now.AddMinutes(ctx.Rng.Next(1, 9));
            if (night && trip.Purpose == LegPurpose.Excursion)
            {
                var town = ctx.NearbyTown(trip.From);
                trip.PlanLeg(LegPurpose.Excursion, $"Gece eğlencesi · {town.Name}", trip.From, town.Location, departAt);
            }
            else
            {
                trip.DepartAt = departAt;
            }
        }

        return chosen.Count;
    }

    private async Task TransferIdleAsync(SimContext ctx, List<Trip> trips, Action<Trip> addTrip, int count, bool night, CancellationToken ct)
    {
        var busy = trips.Where(t => t.State != TripState.Finished).Select(t => t.VehicleId).ToHashSet();
        var due = await reservations.ListDueAsync(ReservationStatus.Confirmed, ctx.Now + PickupHorizon, 1000, ct);
        busy.UnionWith(due.Select(r => r.VehicleId));
        var stock = ctx.StockBranches.Select(b => b.Id).ToHashSet();
        var idle = (await fleet.ListRentableVehiclesAsync(null, ct))
            .Where(v => v.BranchId is { } b && stock.Contains(b) && !busy.Contains(v.Id))
            .OrderBy(_ => ctx.Rng.Next())
            .ToList();

        var started = 0;
        foreach (var candidate in idle)
        {
            if (started >= count)
            {
                break;
            }

            var vehicle = await fleet.GetVehicleAsync(candidate.Id, ct);
            if (vehicle is not { Status: VehicleStatus.Available, BranchId: { } fromId })
            {
                continue;
            }

            var target = TransferTarget(ctx, fromId, night);
            var trip = FleetDispatcher.NewTrip(TripKind.Reposition, vehicle, ctx.Now);
            trip.TargetBranchId = target.Id;
            trip.PlanLeg(LegPurpose.ToOffice, target.Name, vehicle.Position, target.Location, ctx.Now.AddMinutes(ctx.Rng.Next(1, 9)));
            addTrip(trip);
            log.Write(ctx.Now, "reposition", $"{vehicle.Plate} {ctx.Branch(fromId)?.Name} → {target.Name} filo transferi planlandı", vehicle.Id);
            started++;
        }
    }

    /// <summary>Başka bir stok şubesi; havalimanı ağır basar (gece daha da: sabah uçuşları).</summary>
    private static BranchDto TransferTarget(SimContext ctx, Guid fromId, bool night) =>
        ctx.Rng.Pick(
            ctx.StockBranches.Where(b => b.Id != fromId).ToList(),
            b => b.Type == BranchType.AirportDesk ? (night ? 6 : 3) : 2);
}
