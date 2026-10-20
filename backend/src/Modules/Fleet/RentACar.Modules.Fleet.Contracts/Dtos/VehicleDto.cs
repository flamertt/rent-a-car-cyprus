using RentACar.Modules.Fleet.Contracts.Enums;
using RentACar.Shared.Abstractions.Geo;

namespace RentACar.Modules.Fleet.Contracts.Dtos;

public sealed record VehicleDto(
    Guid Id,
    string Plate,
    string Make,
    string Model,
    int Year,
    string Color,
    Guid CategoryId,
    string CategoryCode,
    BodyType Body,
    VehicleStatus Status,
    Guid? BranchId,
    GeoPoint Position,
    double HeadingDeg,
    double SpeedKmh,
    int OdometerKm,
    double FuelLevel,
    DateTimeOffset? LastSeenAt);
