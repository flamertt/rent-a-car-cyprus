using RentACar.Shared.Abstractions.Geo;

namespace RentACar.Modules.Simulation.Domain.ValueObjects;

/// <summary>Konaklama bölgesi ya da gezi noktası (OSM'den).</summary>
internal sealed record Place(string Name, string Kind, string District, GeoPoint Location, double Weight, string? Osm);
