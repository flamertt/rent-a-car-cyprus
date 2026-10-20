using RentACar.Modules.Fleet.Contracts.Enums;

namespace RentACar.Modules.Fleet.Contracts.Dtos;

public sealed record VehicleCategoryDto(
    Guid Id,
    string Code,
    string Name,
    string ExampleModels,
    BodyType Body,
    Transmission Transmission,
    FuelType Fuel,
    int Seats,
    int Doors,
    int Bags,
    int MinDriverAge,
    decimal Deposit,
    int SortOrder);
