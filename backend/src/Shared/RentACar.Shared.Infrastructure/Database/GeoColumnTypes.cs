namespace RentACar.Shared.Infrastructure.Database;

/// <summary>
/// PostGIS kolon tipleri. <c>geography</c> seçildi: LINQ'teki <c>Distance</c>/<c>IsWithinDistance</c>
/// PostGIS'te metre cinsinden çalışır (geometry'de derece olurdu).
/// </summary>
public static class GeoColumnTypes
{
    public const string Point = "geography (point, 4326)";

    public const string LineString = "geography (linestring, 4326)";
}
