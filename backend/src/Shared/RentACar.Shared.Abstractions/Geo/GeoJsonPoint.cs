using System.Text.Json.Serialization;

namespace RentACar.Shared.Abstractions.Geo;

public sealed record GeoJsonPoint(double[] Coordinates)
{
    [JsonPropertyOrder(-1)]
    public string Type => "Point";

    public static GeoJsonPoint From(GeoPoint p) => new([p.Lon, p.Lat]);
}
