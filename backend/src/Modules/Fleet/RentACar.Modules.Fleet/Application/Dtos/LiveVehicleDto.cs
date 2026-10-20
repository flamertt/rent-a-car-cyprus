using RentACar.Modules.Fleet.Contracts.Enums;

namespace RentACar.Modules.Fleet.Application.Dtos;

internal sealed record LiveVehicleDto(
    Guid Id,
    string Plate,
    string CategoryCode,
    BodyType Body,
    string Color,
    VehicleStatus Status,
    double Lon,
    double Lat,
    double HeadingDeg,
    double SpeedKmh,
    double FuelLevel,
    DateTimeOffset? At);
