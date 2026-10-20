using RentACar.Modules.Branches.Contracts.Dtos;
using RentACar.Modules.Branches.Contracts.Enums;
using RentACar.Modules.Fleet.Contracts.Dtos;
using RentACar.Modules.Simulation.Application.Abstractions;
using RentACar.Modules.Simulation.Domain.ValueObjects;
using RentACar.Shared.Abstractions.Geo;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Simulation.Application.Engine;

/// <summary>Bir motor karesinin ortak girdileri (bir kez yüklenir, tüm adımlar paylaşır).</summary>
internal sealed class SimContext(
    DateTimeOffset now,
    IReadOnlyList<VehicleCategoryDto> categories,
    IReadOnlyList<BranchDto> branches,
    IPlacesCatalog places,
    Random rng)
{
    public DateTimeOffset Now { get; } = now;

    public IReadOnlyList<VehicleCategoryDto> Categories { get; } = categories;

    public IReadOnlyList<BranchDto> Branches { get; } = branches;

    public IPlacesCatalog Places { get; } = places;

    public Random Rng { get; } = rng;

    public IReadOnlyList<BranchDto> StockBranches { get; } =
        branches.Where(b => b.Type is BranchType.Office or BranchType.AirportDesk).ToList();

    public BranchDto? Branch(Guid? id) => id is null ? null : Branches.FirstOrDefault(b => b.Id == id);

    public BranchDto NearestStock(GeoPoint p) => StockBranches.MinBy(b => GeoMath.DistanceMeters(p, b.Location))
        ?? throw new InvalidOperationException("Stok tutan şube yok.");

    /// <summary>Akşam yemeği / gece eğlencesi için kasaba ya da tatil bölgesi: yakın ve büyük olan ağır basar, bulunulan yer hariç.</summary>
    public Place NearbyTown(GeoPoint from) =>
        Rng.Pick(
            Places.Stays.Where(s => GeoMath.DistanceMeters(from, s.Location) > 1_500).ToList(),
            s => s.Weight / (1 + GeoMath.DistanceMeters(from, s.Location) / 15_000));

    public DateTimeOffset LocalToUtc(DateOnly date, int hour, int minute)
    {
        var local = date.ToDateTime(new TimeOnly(hour, minute), DateTimeKind.Unspecified);
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, KktcTime.Zone), TimeSpan.Zero);
    }

    /// <summary>
    /// Merkezin <paramref name="radiusMeters"/> çevresinde rastgele nokta (düzgün disk): her müşterinin kendi oteli,
    /// gezi noktasında farklı park yeri — araçlar aynı noktadan aynı rotaya dizilmesin. Rota ucu yola oturur.
    /// </summary>
    public static GeoPoint Scatter(GeoPoint center, double radiusMeters, Random rng)
    {
        var r = radiusMeters * Math.Sqrt(rng.NextDouble());
        var angle = rng.NextDouble() * 2 * Math.PI;
        const double metersPerDegreeLat = 111_320;
        var metersPerDegreeLon = metersPerDegreeLat * Math.Cos(center.Lat * Math.PI / 180);
        return new GeoPoint(center.Lon + r * Math.Sin(angle) / metersPerDegreeLon, center.Lat + r * Math.Cos(angle) / metersPerDegreeLat);
    }

    /// <summary>Yönlendirme yapmadan kaba süre tahmini (planlama için): %30 dolambaç, 55 km/s.</summary>
    public static TimeSpan EstimateTravel(GeoPoint from, GeoPoint to) =>
        TimeSpan.FromHours(GeoMath.DistanceMeters(from, to) * 1.3 / 1000 / 55);
}
