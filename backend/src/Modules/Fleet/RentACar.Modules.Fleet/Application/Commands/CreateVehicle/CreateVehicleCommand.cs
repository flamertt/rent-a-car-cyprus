using RentACar.Modules.Fleet.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Fleet.Application.Commands.CreateVehicle;

internal sealed record CreateVehicleCommand(
    string Plate, string Make, string Model, int Year, string Color, Guid CategoryId, Guid BranchId, int OdometerKm)
    : ICommand<VehicleDto>;
