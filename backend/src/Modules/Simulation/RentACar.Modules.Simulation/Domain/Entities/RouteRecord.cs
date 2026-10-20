using NetTopologySuite.Geometries;

namespace RentACar.Modules.Simulation.Domain.Entities;

/// <summary>Rota önbelleği: aynı başlangıç/bitiş (≈10 m) için yönlendirme servisine tekrar gidilmez.</summary>
internal sealed class RouteRecord
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public string Key { get; set; } = null!;

    public string Provider { get; set; } = null!;

    public LineString Path { get; set; } = null!;

    /// <summary>Ardışık iki koordinat arası süre (s) ve mesafe (m); uzunluk = nokta sayısı − 1.</summary>
    public double[] SegmentDurations { get; set; } = [];

    public double[] SegmentDistances { get; set; } = [];

    public double DistanceMeters { get; set; }

    public double DurationSeconds { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
