using System.Text.Json;
using RentACar.Modules.Simulation.Application.Abstractions;
using RentACar.Modules.Simulation.Domain.ValueObjects;
using RentACar.Shared.Abstractions.Geo;

namespace RentACar.Modules.Simulation.Infrastructure.Places;

/// <summary>Konaklama bölgeleri ve gezi noktaları: OSM'den derlenmiş places.json (gömülü kaynak).</summary>
internal sealed class EmbeddedPlacesCatalog : IPlacesCatalog
{
    private const string ResourceName = "RentACar.Simulation.places.json";

    public EmbeddedPlacesCatalog()
    {
        using var stream = typeof(EmbeddedPlacesCatalog).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"{ResourceName} gömülü kaynağı bulunamadı.");
        using var doc = JsonDocument.Parse(stream);
        Stays = Read(doc.RootElement.GetProperty("stays"), "stay");
        Attractions = Read(doc.RootElement.GetProperty("attractions"), null);
    }

    public IReadOnlyList<Place> Stays { get; }

    public IReadOnlyList<Place> Attractions { get; }

    private static List<Place> Read(JsonElement array, string? kind) =>
        array.EnumerateArray().Select(e => new Place(
            e.GetProperty("name").GetString()!,
            kind ?? e.GetProperty("kind").GetString()!,
            e.TryGetProperty("district", out var d) ? d.GetString()! : "",
            new GeoPoint(e.GetProperty("lon").GetDouble(), e.GetProperty("lat").GetDouble()),
            e.GetProperty("weight").GetDouble(),
            e.GetProperty("osm").GetString())).ToList();
}
