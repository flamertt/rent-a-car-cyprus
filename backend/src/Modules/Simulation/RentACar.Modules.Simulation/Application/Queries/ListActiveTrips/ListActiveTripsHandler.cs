using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Simulation.Application.Abstractions;
using RentACar.Modules.Simulation.Application.Services;
using RentACar.Modules.Simulation.Domain.Enums;
using RentACar.Shared.Abstractions.Geo;
using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Simulation.Application.Queries.ListActiveTrips;

internal sealed class ListActiveTripsHandler(ISimulationDbContext db, RouteMemory routes, IClock clock)
    : IQueryHandler<ListActiveTripsQuery, IReadOnlyList<ActiveTripDto>>
{
    private const int MaxRoutePoints = 300;

    public async Task<IReadOnlyList<ActiveTripDto>> HandleAsync(ListActiveTripsQuery query, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var trips = await db.Trips.AsNoTracking().Where(t => t.State != TripState.Finished).OrderBy(t => t.Plate).ToListAsync(cancellationToken);
        return trips.Select(t =>
        {
            var driving = t.State == TripState.Driving;
            var path = t.RouteId is { } id && routes.ById.TryGetValue(id, out var p) ? p : null;
            var progress = driving && t.LegStartedAt is { } started && t.LegDurationSeconds > 0
                ? Math.Clamp((now - started).TotalSeconds / t.LegDurationSeconds, 0, 1)
                : 0;
            var route = driving && path is not null ? Downsample(path.Points) : [];
            return new ActiveTripDto(t.VehicleId, t.Plate, t.ReservationCode, t.CustomerName, t.Kind.ToString(),
                driving ? "Driving" : "Parked", t.DestinationName, t.StayName, route, progress, t.DepartAt, t.ReturnAt);
        }).ToList();
    }

    private static double[][] Downsample(IReadOnlyList<GeoPoint> points)
    {
        var step = Math.Max(1, (int)Math.Ceiling(points.Count / (double)MaxRoutePoints));
        var result = new List<double[]>();
        for (var i = 0; i < points.Count; i += step)
        {
            result.Add([points[i].Lon, points[i].Lat]);
        }

        if ((points.Count - 1) % step != 0)
        {
            result.Add([points[^1].Lon, points[^1].Lat]);
        }

        return result.ToArray();
    }
}
