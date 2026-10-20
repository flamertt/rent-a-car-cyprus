using NetTopologySuite;
using NetTopologySuite.Geometries;
using RentACar.Shared.Abstractions.Geo;

namespace RentACar.Shared.Infrastructure.Database;

/// <summary>
/// WGS84 (SRID 4326) geometri fabrikası. Konum kolonları <c>geography</c> tipindedir;
/// böylece LINQ'teki <c>Distance</c>/<c>IsWithinDistance</c> PostGIS'te metre cinsinden çalışır.
/// </summary>
public static class Geo
{
    public const int Srid = 4326;

    public const string PointColumnType = "geography (point, 4326)";

    public const string LineColumnType = "geography (linestring, 4326)";

    public static readonly GeometryFactory Factory =
        NtsGeometryServices.Instance.CreateGeometryFactory(srid: Srid);

    public static Point ToPoint(this GeoPoint p) => Factory.CreatePoint(new Coordinate(p.Lon, p.Lat));

    public static GeoPoint ToGeoPoint(this Point p) => new(p.X, p.Y);
}
