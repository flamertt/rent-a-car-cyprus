using RentACar.Shared.Abstractions.Geo;

namespace RentACar.Modules.Fleet.Domain.Services;

/// <summary>
/// Şube otoparkında araç yeri. Araç bir şubeye park ederken o şubedeki ilk boş numarayı alır (<see cref="FirstFree"/>);
/// yerler şube noktasının hemen güneyinden başlayarak satır satır dizilir. Böylece otopark doluluk kadar büyür ve
/// aynı şubedeki iki araç 3B haritada üst üste binmez.
/// </summary>
internal static class ParkingLot
{
    private const int Columns = 10;
    private const double SpacingXMeters = 2.9;
    private const double SpacingYMeters = 5.8;
    private const double MetersPerDegreeLat = 111_320;

    public static GeoPoint SpotFor(GeoPoint center, int slot)
    {
        var index = Math.Max(0, slot);
        var col = index % Columns - (Columns - 1) / 2.0;
        var row = index / Columns + 1;
        var east = col * SpacingXMeters;
        var north = -row * SpacingYMeters;
        var metersPerDegreeLon = MetersPerDegreeLat * Math.Cos(center.Lat * Math.PI / 180);
        return new GeoPoint(center.Lon + east / metersPerDegreeLon, center.Lat + north / MetersPerDegreeLat);
    }

    /// <summary>Dolu yerlerin dışındaki en küçük yer numarası.</summary>
    public static int FirstFree(IEnumerable<int> occupied)
    {
        var taken = occupied as IReadOnlySet<int> ?? occupied.ToHashSet();
        var slot = 0;
        while (taken.Contains(slot))
        {
            slot++;
        }

        return slot;
    }
}
