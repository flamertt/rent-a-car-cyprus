using System.Globalization;
using RentACar.Modules.Fleet.Contracts;
using RentACar.Modules.Fleet.Contracts.Dtos;
using RentACar.Modules.Reservations.Contracts;
using RentACar.Modules.Simulation.Application.Services;
using RentACar.Modules.Simulation.Domain.Entities;
using RentACar.Modules.Simulation.Domain.Enums;
using RentACar.Shared.Abstractions.Exceptions;

namespace RentACar.Modules.Simulation.Application.Engine;

/// <summary>
/// Yolculukları ilerletir: kalkış saati gelen bacağı rotalar, yoldaki araçların konumunu rota üzerinde
/// zamana göre örnekler (telemetri), varışta bir sonraki adımı (gezi / iade / teslim) tetikler. Büyük
/// karelerde (yüksek hız) bir karede birden çok bacak tamamlanabilir.
/// </summary>
internal sealed class TripRunner(RouteService routes, IFleetApi fleet, IReservationsApi reservations, EventLog log)
{
    /// <summary>≈ 6,5 lt/100 km, 45 lt depo → km başına depo oranı.</summary>
    private const double FuelPerKm = 0.065 / 45;
    private const int MaxTransitionsPerTick = 6;
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

    public async Task RunAsync(SimContext ctx, SimState state, IEnumerable<Trip> trips, CancellationToken ct)
    {
        var telemetry = new Dictionary<Guid, VehicleTelemetryDto>();
        foreach (var trip in trips.Where(t => t.State != TripState.Finished))
        {
            try
            {
                await AdvanceAsync(ctx, state, trip, telemetry, ct);
            }
            catch (DomainException ex)
            {
                log.Write(ctx.Now, "error", $"{trip.Plate}: {ex.Message}", trip.VehicleId, trip.ReservationCode);
                trip.Finish(ctx.Now);
            }
        }

        if (telemetry.Count > 0)
        {
            await fleet.ReportTelemetryAsync(telemetry.Values.ToList(), ct);
        }
    }

    private async Task AdvanceAsync(SimContext ctx, SimState state, Trip trip, Dictionary<Guid, VehicleTelemetryDto> telemetry, CancellationToken ct)
    {
        for (var i = 0; i < MaxTransitionsPerTick && trip.State != TripState.Finished; i++)
        {
            if (trip.State == TripState.Waiting)
            {
                if (trip.DepartAt > ctx.Now)
                {
                    return;
                }

                if (trip.Kind != TripKind.Rental && trip.LegStartedAt is null && trip.Purpose is LegPurpose.ToPickupBranch or LegPurpose.ToOffice or LegPurpose.ToService)
                {
                    await fleet.SetInTransitAsync(trip.VehicleId, true, null, ct);
                }

                var route = await routes.GetAsync(trip.From, trip.To, ct);
                trip.RouteId = route.Id;
                trip.LegStartedAt = trip.DepartAt;
                trip.LegDurationSeconds = route.DurationSeconds;
                trip.LegDistanceMeters = route.DistanceMeters;
                trip.State = TripState.Driving;
            }

            var path = await routes.GetByIdOrResolveAsync(trip.RouteId, trip.From, trip.To, ct);
            var elapsed = (ctx.Now - trip.LegStartedAt!.Value).TotalSeconds;
            if (elapsed < path.DurationSeconds)
            {
                var s = path.Sample(elapsed);
                var km = s.TraveledMeters / 1000;
                telemetry[trip.VehicleId] = new VehicleTelemetryDto(
                    trip.VehicleId, s.Position, s.HeadingDeg, s.SpeedKmh,
                    (int)(trip.OdometerKm + km), trip.FuelLevel - km * FuelPerKm, ctx.Now);
                return;
            }

            // Varış.
            var arrivedAt = trip.LegStartedAt.Value.AddSeconds(path.DurationSeconds);
            var legKm = path.DistanceMeters / 1000;
            trip.OdometerKm += legKm;
            trip.FuelLevel = Math.Max(0.02, trip.FuelLevel - legKm * FuelPerKm);
            if (trip.Kind == TripKind.Rental && trip.FuelLevel < 0.15)
            {
                trip.FuelLevel = 0.9 + ctx.Rng.NextDouble() * 0.1;
                log.Write(ctx.Now, "fuel", $"{trip.Plate} yakıt aldı ({trip.CustomerName})", trip.VehicleId, trip.ReservationCode);
            }

            // Yolun bittiği nokta (yola oturmuş) bir sonraki bacağın başlangıcı olur.
            trip.ToLon = path.End.Lon;
            trip.ToLat = path.End.Lat;
            telemetry[trip.VehicleId] = new VehicleTelemetryDto(
                trip.VehicleId, path.End, path.Sample(path.DurationSeconds).HeadingDeg, 0, (int)trip.OdometerKm, trip.FuelLevel, arrivedAt);

            await OnArrivalAsync(ctx, state, trip, arrivedAt, ct);
        }
    }

    private async Task OnArrivalAsync(SimContext ctx, SimState state, Trip trip, DateTimeOffset arrivedAt, CancellationToken ct)
    {
        switch (trip.Purpose)
        {
            case LegPurpose.ToPickupBranch:
            case LegPurpose.ToOffice:
            case LegPurpose.FromService:
                await fleet.SetInTransitAsync(trip.VehicleId, false, trip.TargetBranchId, ct);
                trip.Finish(ctx.Now);
                log.Write(ctx.Now, trip.Kind == TripKind.Delivery ? "delivered" : "repositioned",
                    $"{trip.Plate} vardı · {trip.DestinationName}", trip.VehicleId, trip.ReservationCode);
                break;

            case LegPurpose.ToService:
                var home = ctx.Branch(trip.TargetBranchId) ?? ctx.NearestStock(trip.To);
                trip.PlanLeg(LegPurpose.FromService, home.Name, trip.To, home.Location, arrivedAt.AddMinutes(ctx.Rng.Next(20, 60)));
                break;

            case LegPurpose.ToReturnBranch:
                // Müşterilerin çoğu aracı aldığı yakıt seviyesinde bırakır; kalanına eksik yakıt yansır.
                if (ctx.Rng.Chance(0.7))
                {
                    trip.FuelLevel = Math.Max(trip.FuelLevel, trip.StartFuelLevel);
                }

                var returned = await reservations.ReturnAsync(trip.ReservationId!.Value, trip.TargetBranchId, (int)Math.Ceiling(trip.OdometerKm), trip.FuelLevel, ct);
                state.Returns++;
                trip.Finish(ctx.Now);
                var extras = returned.Lines.Where(l => l.Code is "LATE_RETURN" or "REFUEL").Select(l => l.Description).ToList();
                log.Write(ctx.Now, "return",
                    $"{trip.Plate} iade edildi · {trip.DestinationName} · {returned.CustomerName} · {returned.Total.ToString("N0", Tr)} TL" +
                    (extras.Count > 0 ? $" ({string.Join(", ", extras)})" : ""),
                    trip.VehicleId, returned.Code);
                break;

            default:
                ItineraryPlanner.PlanNext(trip, ctx, arrivedAt);
                break;
        }
    }
}
