using RentACar.Modules.Fleet.Contracts.Dtos;
using RentACar.Modules.Fleet.Contracts.Enums;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Fleet.Application.Commands.ChangeVehicleStatus;

internal sealed record ChangeVehicleStatusCommand(Guid VehicleId, VehicleStatus Status) : ICommand<VehicleDto>;
