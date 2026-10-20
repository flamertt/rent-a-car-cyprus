using RentACar.Modules.Simulation.Application.Abstractions;
using RentACar.Shared.Abstractions.Geo;

namespace RentACar.Modules.Simulation.Infrastructure.Routing;

/// <summary>
/// Yönlendirme servisi yokken yedek: düz çizgi, ~300 m'lik ara noktalar, %30 dolambaç payı ve 50 km/s.
/// Haritada yollara oturmaz; yalnız simülasyonun durmaması için.
/// </summary>
internal static class StraightLineRouter
{
    private const double DetourFactor = 1.3;
    private const double SpeedMetersPerSecond = 50 / 3.6;
    private const double StepMeters = 300;

    public static RouteResult Route(GeoPoint from, GeoPoint to)
    {
        var direct = Math.Max(1, GeoMath.DistanceMeters(from, to));
        var steps = Math.Max(1, (int)Math.Ceiling(direct / StepMeters));
        var points = Enumerable.Range(0, steps + 1).Select(i => GeoMath.Lerp(from, to, i / (double)steps)).ToArray();
        var segment = direct * DetourFactor / steps;
        return new RouteResult(
            "line",
            points,
            Enumerable.Repeat(segment / SpeedMetersPerSecond, steps).ToArray(),
            Enumerable.Repeat(segment, steps).ToArray());
    }
}
