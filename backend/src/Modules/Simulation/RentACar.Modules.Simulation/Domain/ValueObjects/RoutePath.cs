using RentACar.Shared.Abstractions.Geo;

namespace RentACar.Modules.Simulation.Domain.ValueObjects;

/// <summary>
/// Yol geometrisi + segment süreleri. Konum zamana göre örneklenir: yönlendirme servisinin segment
/// süreleri (yol sınıfı/hız) kullanıldığı için araç otoyolda hızlı, köy yolunda yavaş ilerler.
/// </summary>
internal sealed class RoutePath
{
    private readonly GeoPoint[] _points;
    private readonly double[] _cumTime;
    private readonly double[] _cumDist;

    public RoutePath(Guid id, string provider, GeoPoint[] points, double[] segmentDurations, double[] segmentDistances)
    {
        if (points.Length < 2 || segmentDurations.Length != points.Length - 1 || segmentDistances.Length != points.Length - 1)
        {
            throw new ArgumentException("Rota en az iki nokta ve nokta−1 segment içermeli.");
        }

        Id = id;
        Provider = provider;
        _points = points;
        _cumTime = new double[points.Length];
        _cumDist = new double[points.Length];
        for (var i = 1; i < points.Length; i++)
        {
            _cumTime[i] = _cumTime[i - 1] + Math.Max(0, segmentDurations[i - 1]);
            _cumDist[i] = _cumDist[i - 1] + Math.Max(0, segmentDistances[i - 1]);
        }
    }

    public Guid Id { get; }

    public string Provider { get; }

    public IReadOnlyList<GeoPoint> Points => _points;

    public double DurationSeconds => _cumTime[^1];

    public double DistanceMeters => _cumDist[^1];

    public GeoPoint End => _points[^1];

    public RouteSample Sample(double elapsedSeconds)
    {
        if (elapsedSeconds <= 0)
        {
            return new RouteSample(_points[0], GeoMath.BearingDeg(_points[0], _points[1]), 0, 0);
        }

        if (elapsedSeconds >= DurationSeconds)
        {
            return new RouteSample(_points[^1], GeoMath.BearingDeg(_points[^2], _points[^1]), 0, DistanceMeters);
        }

        // İkili arama: elapsed'i içeren segment.
        var i = Array.BinarySearch(_cumTime, elapsedSeconds);
        var seg = i >= 0 ? Math.Min(i, _points.Length - 2) : ~i - 1;
        var segTime = _cumTime[seg + 1] - _cumTime[seg];
        var t = segTime <= 0 ? 1 : (elapsedSeconds - _cumTime[seg]) / segTime;
        var a = _points[seg];
        var b = _points[seg + 1];
        var segDist = _cumDist[seg + 1] - _cumDist[seg];
        var speed = segTime <= 0 ? 0 : segDist / segTime * 3.6;
        return new RouteSample(GeoMath.Lerp(a, b, t), GeoMath.BearingDeg(a, b), speed, _cumDist[seg] + segDist * t);
    }
}
