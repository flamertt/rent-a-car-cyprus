using System.Text.Json.Serialization;

namespace RentACar.Shared.Abstractions.Geo;

public sealed record GeoJsonFeature(string Id, GeoJsonPoint Geometry, IReadOnlyDictionary<string, object?> Properties)
{
    [JsonPropertyOrder(-1)]
    public string Type => "Feature";
}
