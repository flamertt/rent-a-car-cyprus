using RentACar.Shared.Abstractions.Geo;

namespace RentACar.Modules.Fleet.Contracts.Dtos;

public sealed record VehicleTelemetryDto(
    Guid VehicleId,
    GeoPoint Position,
    double HeadingDeg,
    double SpeedKmh,
    int OdometerKm,
    double FuelLevel,
    DateTimeOffset At);
