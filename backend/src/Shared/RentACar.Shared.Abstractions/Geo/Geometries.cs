using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace RentACar.Shared.Abstractions.Geo;

/// <summary>
/// WGS84 (SRID 4326) geometri fabrikası ve <see cref="GeoPoint"/> ↔ NetTopologySuite dönüşümleri.
/// Domain varlıkları PostGIS kolonlarını NTS tipleriyle tutar; NTS saf bir geometri kütüphanesidir (EF'e bağlı değil).
/// </summary>
public static class Geometries
{
    public const int Srid = 4326;

    public static readonly GeometryFactory Factory =
        NtsGeometryServices.Instance.CreateGeometryFactory(srid: Srid);

    public static Point ToPoint(this GeoPoint p) => Factory.CreatePoint(new Coordinate(p.Lon, p.Lat));

    public static GeoPoint ToGeoPoint(this Point p) => new(p.X, p.Y);

    public static LineString ToLineString(this IEnumerable<GeoPoint> points) =>
        Factory.CreateLineString(points.Select(p => new Coordinate(p.Lon, p.Lat)).ToArray());
}
