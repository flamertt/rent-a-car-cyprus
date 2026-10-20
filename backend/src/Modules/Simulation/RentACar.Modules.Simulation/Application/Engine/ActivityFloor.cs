using Microsoft.Extensions.Options;
using RentACar.Modules.Fleet.Contracts;
using RentACar.Modules.Fleet.Contracts.Enums;
using RentACar.Modules.Reservations.Contracts;
using RentACar.Modules.Reservations.Contracts.Enums;
using RentACar.Modules.Simulation.Application.Options;
using RentACar.Modules.Simulation.Domain.Entities;
using RentACar.Modules.Simulation.Domain.Enums;
using RentACar.Shared.Abstractions.Geo;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Simulation.Application.Engine;

/// <summary>
/// Asgari hareket (güçlü filo varsayımı): haritada her an en az <see cref="SimulationOptions.MinMovingVehicles"/> araç
/// yolda olsun. Hareketin asıl kaynağı müşterilerdir (bkz. <see cref="SimulationOptions.TargetUtilization"/>); burası
/// yalnız açığı doğal görünen yollarla kapatır:
/// <list type="bullet">
/// <item>gündüz: yakında (2 saat içinde) zaten çıkacak müşteri biraz erken yola çıkar; ertesi sabahı bekleyen park hâlinde kalır,</item>
/// <item>akşam (20–01): konaklamadaki müşterinin bir kısmı kasabaya eğlenceye çıkar,</item>
/// <item>yetmezse boştaki araçlar farklı kasabalara servis turuna (yıkama/bakım) gidip şubesine döner.</item>
/// </list>
/// Konvoy olmasın diye her karede az sayıda, aynı şubeden 1–3 dk arayla ve farklı kasabalara kalkış planlanır.
/// Hızlı ileri sarma (yetişme) sırasında çağrılmaz.
/// </summary>
internal sealed class ActivityFloor(IReservationsApi reservations, IFleetApi fleet, IOptions<SimulationOptions> options, EventLog log)
{
    /// <summary>Bir operasyon karesinde (≈2 sn) en çok bu kadar yeni kalkış.</summary>
    private const int MaxStartsPerRun = 2;

    /// <summary>Kalkmak üzere olanlar da sayıldığı için hedefe pay: yoldaki araç asgarinin altına düşmesin.</summary>
    private const double Headroom = 1.1;

    /// <summary>Servis noktasının kasaba merkezine en uzak mesafesi.</summary>
    private const double ServiceSpreadMeters = 1_500;

    /// <summary>Bu kadar içinde kalkacak bacak "yolda" sayılır.</summary>
    private static readonly TimeSpan Imminent = TimeSpan.FromMinutes(5);

    /// <summary>Gündüz yalnız bu kadar içinde zaten yola çıkacak müşteri öne çekilir.</summary>
    private static readonly TimeSpan PullForwardWindow = TimeSpan.FromHours(2);

    /// <summary>İadesine bundan az kalan müşteri ek geziye çıkarılmaz.</summary>
    private static readonly TimeSpan ReturnMargin = TimeSpan.FromHours(4);

    /// <summary>Teslimi bu kadar içinde olan araç servis turuna çıkarılmaz.</summary>
    private static readonly TimeSpan PickupHorizon = TimeSpan.FromHours(12);

    public static int CountMoving(IEnumerable<Trip> trips, DateTimeOffset now) =>
        trips.Count(t => t.State == TripState.Driving || (t.State == TripState.Waiting && t.DepartAt <= now + Imminent));

    public async Task RunAsync(SimContext ctx, List<Trip> trips, Action<Trip> addTrip, CancellationToken ct)
    {
        var target = (int)Math.Ceiling(options.Value.MinMovingVehicles * Headroom);
        var deficit = Math.Min(target - CountMoving(trips, ctx.Now), MaxStartsPerRun);
        if (deficit <= 0)
        {
            return;
        }

        var started = SendCustomersOut(ctx, trips, deficit);
        if (started < deficit)
        {
            await SendIdleToServiceAsync(ctx, trips, addTrip, deficit - started, ct);
        }
    }

    /// <summary>Müşteri bacağını öne çeker ya da (akşam) kasabaya eğlenceye çıkarır. Dönüş ve iade <see cref="ItineraryPlanner"/>'da.</summary>
    internal static int SendCustomersOut(SimContext ctx, IEnumerable<Trip> trips, int count)
    {
        if (count <= 0)
        {
            return 0;
        }

        var hour = KktcTime.ToLocal(ctx.Now).Hour;
        var evening = hour is >= 20 or < 1;
        var daytime = hour is >= 7 and < 20;
        var parked = trips
            .Where(t => t is { Kind: TripKind.Rental, State: TripState.Waiting, Purpose: LegPurpose.Excursion or LegPurpose.BackToStay }
                && t.DepartAt > ctx.Now + Imminent
                && t.ReturnAt - ctx.Now > ReturnMargin)
            .ToList();

        IEnumerable<Trip> chosen = [];
        if (daytime)
        {
            chosen = parked.Where(t => t.DepartAt <= ctx.Now + PullForwardWindow);
        }
        else if (evening)
        {
            // Konaklamada (bir sonraki gezisi yarın sabah) olanlardan.
            chosen = parked.Where(t => t.Purpose == LegPurpose.Excursion && t.DepartAt > ctx.Now + TimeSpan.FromHours(6));
        }

        var picked = chosen.OrderBy(_ => ctx.Rng.Next()).Take(count).ToList();
        foreach (var trip in picked)
        {
            var departAt = ctx.Now.AddMinutes(ctx.Rng.Next(2, 13));
            if (evening)
            {
                var town = ctx.NearbyTown(trip.From);
                var spot = SimContext.Scatter(town.Location, ItineraryPlanner.TownSpreadMeters, ctx.Rng);
                trip.PlanLeg(LegPurpose.Excursion, $"Gece eğlencesi · {town.Name}", trip.From, spot, departAt);
            }
            else
            {
                trip.DepartAt = departAt;
            }
        }

        return picked.Count;
    }

    /// <summary>
    /// Boştaki araç (yakında teslimi yok) servis turuna çıkar: yakın bir kasabadaki servis noktası → bekleme → aynı şube.
    /// Aynı şubeden kalkışlar arasında 1–3 dk olur, aynı şubeden aynı kasabaya aynı anda iki tur gitmez.
    /// </summary>
    private async Task SendIdleToServiceAsync(SimContext ctx, List<Trip> trips, Action<Trip> addTrip, int count, CancellationToken ct)
    {
        var active = trips.Where(t => t.State != TripState.Finished).ToList();
        var busy = active.Select(t => t.VehicleId).ToHashSet();
        var due = await reservations.ListDueAsync(ReservationStatus.Confirmed, ctx.Now + PickupHorizon, 500, ct);
        busy.UnionWith(due.Select(r => r.VehicleId));
        var stock = ctx.StockBranches.Select(b => b.Id).ToHashSet();
        var idle = (await fleet.ListRentableVehiclesAsync(null, ct))
            .Where(v => v.BranchId is { } b && stock.Contains(b) && !busy.Contains(v.Id))
            .OrderBy(_ => ctx.Rng.Next())
            .ToList();

        var services = active.Where(t => t.Purpose == LegPurpose.ToService).ToList();
        var started = 0;
        foreach (var candidate in idle)
        {
            if (started >= count)
            {
                break;
            }

            var vehicle = await fleet.GetVehicleAsync(candidate.Id, ct);
            if (vehicle is not { Status: VehicleStatus.Available, BranchId: { } branchId } || ctx.Branch(branchId) is not { } branch)
            {
                continue;
            }

            // Aynı şubeden son kalkışın ardından 1–3 dk.
            var lastFromHere = services
                .Where(t => t.TargetBranchId == branchId && t.State == TripState.Waiting)
                .Select(t => (DateTimeOffset?)t.DepartAt)
                .Max();
            var departAt = Max(ctx.Now.AddMinutes(ctx.Rng.Next(1, 4)), (lastFromHere ?? ctx.Now).AddMinutes(ctx.Rng.Next(1, 4)));

            var takenTowns = services.Where(t => t.TargetBranchId == branchId).Select(t => t.DestinationName).ToHashSet();
            var towns = ctx.Places.Stays.Where(s => !takenTowns.Contains(ServiceName(s.Name))).ToList();
            if (towns.Count == 0)
            {
                continue;
            }

            var town = ctx.Rng.Pick(towns, s => s.Weight / (1 + GeoMath.DistanceMeters(branch.Location, s.Location) / 15_000));
            var trip = FleetDispatcher.NewTrip(TripKind.Reposition, vehicle, ctx.Now);
            trip.TargetBranchId = branchId;
            trip.PlanLeg(LegPurpose.ToService, ServiceName(town.Name), vehicle.Position,
                SimContext.Scatter(town.Location, ServiceSpreadMeters, ctx.Rng), departAt);
            addTrip(trip);
            services.Add(trip);
            log.Write(ctx.Now, "reposition", $"{vehicle.Plate} {branch.Name} → servis turu ({town.Name})", vehicle.Id);
            started++;
        }
    }

    private static string ServiceName(string town) => $"Servis · {town}";

    private static DateTimeOffset Max(DateTimeOffset a, DateTimeOffset b) => a > b ? a : b;
}
