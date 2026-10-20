using System.Text.Json.Serialization;

namespace RentACar.Shared.Abstractions.Geo;

/// <summary>WGS84 nokta. Sözleşmelerde NetTopologySuite'e bağımlı olmamak için kullanılır.</summary>
public readonly record struct GeoPoint(double Lon, double Lat)
{
    /// <summary>KKTC'yi kabaca kapsayan kutu (Yeşil Hat'ın kuzeyi + Karpaz).</summary>
    public static readonly (double MinLon, double MinLat, double MaxLon, double MaxLat) KktcBounds =
        (32.60, 35.00, 34.65, 35.75);

    public bool IsInsideKktcBounds =>
        Lon >= KktcBounds.MinLon && Lon <= KktcBounds.MaxLon && Lat >= KktcBounds.MinLat && Lat <= KktcBounds.MaxLat;
}

/// <summary>MapLibre'ın doğrudan okuyabileceği en küçük GeoJSON modeli.</summary>
public sealed record GeoJsonFeatureCollection(IReadOnlyList<GeoJsonFeature> Features)
{
    [JsonPropertyOrder(-1)]
    public string Type => "FeatureCollection";
}

public sealed record GeoJsonFeature(string Id, GeoJsonPoint Geometry, IReadOnlyDictionary<string, object?> Properties)
{
    [JsonPropertyOrder(-1)]
    public string Type => "Feature";
}

public sealed record GeoJsonPoint(double[] Coordinates)
{
    [JsonPropertyOrder(-1)]
    public string Type => "Point";

    public static GeoJsonPoint From(GeoPoint p) => new([p.Lon, p.Lat]);
}
