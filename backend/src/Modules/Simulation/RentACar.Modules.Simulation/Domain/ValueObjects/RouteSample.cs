using RentACar.Shared.Abstractions.Geo;

namespace RentACar.Modules.Simulation.Domain.ValueObjects;

internal readonly record struct RouteSample(GeoPoint Position, double HeadingDeg, double SpeedKmh, double TraveledMeters);
