using RentACar.Modules.Fleet.Contracts.Dtos;
using RentACar.Modules.Fleet.Contracts.Enums;
using RentACar.Shared.Abstractions.Common;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Fleet.Application.Queries.ListVehicles;

internal sealed record ListVehiclesQuery(
    VehicleStatus? Status, Guid? CategoryId, Guid? BranchId, string? Search, int? Page, int? PageSize)
    : IQuery<PagedResult<VehicleDto>>;
