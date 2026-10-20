using System.Globalization;
using RentACar.Modules.Branches.Contracts.Enums;
using RentACar.Modules.Fleet.Contracts;
using RentACar.Modules.Fleet.Contracts.Dtos;
using RentACar.Modules.Fleet.Contracts.Enums;
using RentACar.Modules.Reservations.Contracts;
using RentACar.Modules.Reservations.Contracts.Dtos;
using RentACar.Modules.Reservations.Contracts.Enums;
using RentACar.Modules.Simulation.Domain.Entities;
using RentACar.Modules.Simulation.Domain.Enums;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Simulation.Application.Engine;

/// <summary>
/// Operasyon ekibi: teslim zamanı gelen rezervasyon için aracı teslim noktasına götürür, müşteri gelince
/// aracı teslim eder; her aktif kiralamanın bir yolculuğu olmasını sağlar; stoksuz noktada (liman, sınır
/// kapısı) kalan aracı en yakın ofise geri getirir.
/// </summary>
internal sealed class FleetDispatcher(IReservationsApi reservations, IFleetApi fleet, EventLog log)
{
    private const double NoShowProbability = 0.04;
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

    public async Task RunAsync(SimContext ctx, SimState state, List<Trip> trips, Action<Trip> addTrip, CancellationToken ct)
    {
        var due = await reservations.ListDueAsync(ReservationStatus.Confirmed, ctx.Now.AddHours(12), 300, ct);
        foreach (var r in due)
        {
            await HandleConfirmedAsync(ctx, state, r, trips, addTrip, ct);
        }

        var active = await reservations.ListDueAsync(ReservationStatus.Active, ctx.Now.AddYears(1), 500, ct);
        foreach (var r in active.Where(r => !trips.Any(t => t.ReservationId == r.Id && t.Kind == TripKind.Rental)))
        {
            var vehicle = await fleet.GetVehicleAsync(r.VehicleId, ct);
            if (vehicle is not null)
            {
                addTrip(NewRentalTrip(ctx, r, vehicle));
            }
        }

        await RepositionAsync(ctx, due, trips, addTrip, ct);
    }

    private async Task HandleConfirmedAsync(
        SimContext ctx, SimState state, ReservationDto r, List<Trip> trips, Action<Trip> addTrip, CancellationToken ct)
    {
        var decision = SimRandom.For(r.Id);
        var noShow = decision.Chance(NoShowProbability);
        var customerDelay = TimeSpan.FromMinutes(decision.Next(-15, 45));
        var branch = ctx.Branch(r.PickupBranchId);
        if (branch is null || trips.Any(t => t.VehicleId == r.VehicleId && t.Kind != TripKind.Rental))
        {
            return; // araç zaten yolda (teslimat/transfer) — varınca tekrar bakılır
        }

        var vehicle = await fleet.GetVehicleAsync(r.VehicleId, ct);
        if (vehicle is null || vehicle.Status != VehicleStatus.Available)
        {
            return; // önceki müşteri henüz dönmedi ya da bakımda; gelmedi taraması ya da iade çözer
        }

        if (vehicle.BranchId != r.PickupBranchId)
        {
            var eta = SimContext.EstimateTravel(vehicle.Position, branch.Location);
            var departAt = r.PickupAt - eta - TimeSpan.FromMinutes(30);
            if (departAt > ctx.Now.AddHours(1))
            {
                return; // henüz erken; araç şubede kalsın
            }

            var trip = NewTrip(TripKind.Delivery, vehicle, ctx.Now);
            trip.ReservationId = r.Id;
            trip.ReservationCode = r.Code;
            trip.CustomerName = r.CustomerName;
            trip.TargetBranchId = branch.Id;
            trip.PlanLeg(LegPurpose.ToPickupBranch, branch.Name, vehicle.Position, branch.Location, departAt < ctx.Now ? ctx.Now : departAt);
            addTrip(trip);
            log.Write(ctx.Now, "delivery", $"{vehicle.Plate} → {branch.Name} teslimatı planlandı ({r.Code}, teslim {Local(r.PickupAt)})", vehicle.Id, r.Code);
            return;
        }

        if (noShow || ctx.Now < r.PickupAt + customerDelay)
        {
            return;
        }

        try
        {
            var picked = await reservations.PickUpAsync(r.Id, vehicle.OdometerKm, vehicle.FuelLevel, ct);
            state.Pickups++;
            addTrip(NewRentalTrip(ctx, picked, vehicle));
            log.Write(ctx.Now, "pickup", $"{vehicle.Plate} teslim edildi · {branch.Name} · {picked.CustomerName} ({picked.Code}, iade {Local(picked.ReturnAt)})", vehicle.Id, picked.Code);
        }
        catch (DomainException ex)
        {
            log.Write(ctx.Now, "error", $"{r.Code} teslim edilemedi: {ex.Message}", vehicle.Id, r.Code);
        }
    }

    private static Trip NewRentalTrip(SimContext ctx, ReservationDto r, VehicleDto vehicle)
    {
        var stay = SimRandom.For(r.CustomerId).Pick(ctx.Places.Stays, s => s.Weight);
        var trip = NewTrip(TripKind.Rental, vehicle, ctx.Now);
        trip.ReservationId = r.Id;
        trip.ReservationCode = r.Code;
        trip.CustomerName = r.CustomerName;
        trip.TargetBranchId = r.ReturnBranchId;
        trip.ReturnAt = r.ReturnAt;
        trip.StayName = stay.Name;
        trip.StayLon = stay.Location.Lon;
        trip.StayLat = stay.Location.Lat;
        trip.PlanLeg(LegPurpose.ToStay, stay.Name, vehicle.Position, stay.Location, ctx.Now.AddMinutes(10));
        return trip;
    }

    /// <summary>Stoksuz noktada bekleyen ve yakında orada teslimi olmayan araçlar en yakın ofise döner.</summary>
    private async Task RepositionAsync(SimContext ctx, IReadOnlyList<ReservationDto> due, List<Trip> trips, Action<Trip> addTrip, CancellationToken ct)
    {
        var nonStock = ctx.Branches.Where(b => b.Type is not (BranchType.Office or BranchType.AirportDesk)).Select(b => b.Id).ToHashSet();
        var waitingHere = due.Select(r => r.VehicleId).ToHashSet();
        foreach (var v in await fleet.ListRentableVehiclesAsync(null, ct))
        {
            if (v.BranchId is not { } branchId || !nonStock.Contains(branchId) || waitingHere.Contains(v.Id) || trips.Any(t => t.VehicleId == v.Id))
            {
                continue;
            }

            var vehicle = await fleet.GetVehicleAsync(v.Id, ct);
            if (vehicle is null || vehicle.Status != VehicleStatus.Available)
            {
                continue;
            }

            var office = ctx.NearestStock(vehicle.Position);
            var trip = NewTrip(TripKind.Reposition, vehicle, ctx.Now);
            trip.TargetBranchId = office.Id;
            trip.PlanLeg(LegPurpose.ToOffice, office.Name, vehicle.Position, office.Location, ctx.Now.AddMinutes(30));
            addTrip(trip);
            log.Write(ctx.Now, "reposition", $"{vehicle.Plate} {ctx.Branch(branchId)?.Name} → {office.Name} transferi planlandı", vehicle.Id);
        }
    }

    internal static Trip NewTrip(TripKind kind, VehicleDto vehicle, DateTimeOffset now) => new()
    {
        Kind = kind,
        VehicleId = vehicle.Id,
        Plate = vehicle.Plate,
        OdometerKm = vehicle.OdometerKm,
        FuelLevel = vehicle.FuelLevel,
        StartFuelLevel = vehicle.FuelLevel,
        CreatedAt = now,
    };

    private static string Local(DateTimeOffset utc) => KktcTime.ToLocal(utc).ToString("d MMM HH:mm", Tr);
}
