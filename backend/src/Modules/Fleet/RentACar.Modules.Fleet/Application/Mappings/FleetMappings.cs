using RentACar.Modules.Fleet.Application.Dtos;
using RentACar.Modules.Fleet.Contracts.Dtos;
using RentACar.Modules.Fleet.Domain.Entities;
using RentACar.Shared.Abstractions.Geo;

namespace RentACar.Modules.Fleet.Application.Mappings;

internal static class FleetMappings
{
    public static VehicleCategoryDto ToDto(this VehicleCategory c) => new(
        c.Id, c.Code, c.Name, c.ExampleModels, c.Body, c.Transmission, c.Fuel,
        c.Seats, c.Doors, c.Bags, c.MinDriverAge, c.Deposit, c.SortOrder);

    /// <summary><paramref name="v"/>.Category yüklü olmalı (Include).</summary>
    public static VehicleDto ToDto(this Vehicle v) => new(
        v.Id, v.Plate, v.Make, v.Model, v.Year, v.Color, v.CategoryId, v.Category.Code, v.Category.Body,
        v.Status, v.BranchId, v.Position.ToGeoPoint(), v.HeadingDeg, v.SpeedKmh, v.OdometerKm, v.FuelLevel, v.LastSeenAt);

    /// <summary><paramref name="v"/>.Category yüklü olmalı (Include).</summary>
    public static LiveVehicleDto ToLive(this Vehicle v) => new(
        v.Id, v.Plate, v.Category.Code, v.Category.Body, v.Color, v.Status,
        v.Position.X, v.Position.Y, v.HeadingDeg, v.SpeedKmh, v.FuelLevel, v.LastSeenAt);
}
