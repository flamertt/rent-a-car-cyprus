using RentACar.Modules.Fleet.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Fleet.Application.Queries.ListRentableVehicles;

internal sealed record ListRentableVehiclesQuery(Guid? CategoryId) : IQuery<IReadOnlyList<RentableVehicleDto>>;
