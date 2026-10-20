using RentACar.Modules.Fleet.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Fleet.Application.Queries.GetVehicle;

internal sealed record GetVehicleQuery(Guid Id) : IQuery<VehicleDto?>;
