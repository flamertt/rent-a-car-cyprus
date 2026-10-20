using System.Text.Json.Serialization;

namespace RentACar.Shared.Abstractions.Geo;

/// <summary>MapLibre'ın doğrudan okuyabileceği en küçük GeoJSON modeli.</summary>
public sealed record GeoJsonFeatureCollection(IReadOnlyList<GeoJsonFeature> Features)
{
    [JsonPropertyOrder(-1)]
    public string Type => "FeatureCollection";
}
