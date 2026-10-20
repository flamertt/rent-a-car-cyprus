using RentACar.Modules.Simulation.Domain.ValueObjects;
using RentACar.Modules.Simulation.Infrastructure.Routing;
using RentACar.Shared.Abstractions.Geo;

namespace RentACar.UnitTests.Simulation;

public sealed class RoutingTests
{
    [Fact]
    public void Route_is_sampled_by_segment_time_not_distance()
    {
        // 1. segment 1 km / 60 s (60 km/s), 2. segment 1 km / 120 s (30 km/s).
        var a = new GeoPoint(33.0, 35.0);
        var b = new GeoPoint(33.011, 35.0);
        var c = new GeoPoint(33.022, 35.0);
        var path = new RoutePath(Guid.NewGuid(), "test", [a, b, c], [60, 120], [1_000, 1_000]);

        Assert.Equal(180, path.DurationSeconds);
        Assert.Equal(2_000, path.DistanceMeters);

        var atB = path.Sample(60);
        Assert.Equal(b.Lon, atB.Position.Lon, 6);

        var mid2 = path.Sample(120); // ikinci segmentin yarısı
        Assert.Equal((b.Lon + c.Lon) / 2, mid2.Position.Lon, 6);
        Assert.Equal(30, mid2.SpeedKmh, 3);
        Assert.Equal(1_500, mid2.TraveledMeters, 3);
        Assert.Equal(90, mid2.HeadingDeg, 0); // doğu

        Assert.Equal(c, path.Sample(999).Position);
    }

    [Fact]
    public void Straight_line_fallback_keeps_endpoints_and_detour()
    {
        var from = new GeoPoint(33.3189, 35.3372); // Girne
        var to = new GeoPoint(33.5037, 35.1484);   // Ercan
        var result = StraightLineRouter.Route(from, to);
        var direct = GeoMath.DistanceMeters(from, to);

        Assert.Equal(from, result.Points[0]);
        Assert.Equal(to, result.Points[^1]);
        Assert.Equal(direct * 1.3, result.SegmentDistances.Sum(), 0);
        Assert.InRange(direct, 25_000, 28_000); // kuş uçuşu ~26 km
    }
}
