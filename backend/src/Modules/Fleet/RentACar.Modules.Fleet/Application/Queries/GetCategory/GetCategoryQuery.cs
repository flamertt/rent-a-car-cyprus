using RentACar.Modules.Fleet.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Fleet.Application.Queries.GetCategory;

internal sealed record GetCategoryQuery(Guid Id) : IQuery<VehicleCategoryDto?>;
