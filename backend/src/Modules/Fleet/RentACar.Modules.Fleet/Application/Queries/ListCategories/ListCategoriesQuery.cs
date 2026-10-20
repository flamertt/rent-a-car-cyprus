using RentACar.Modules.Fleet.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Fleet.Application.Queries.ListCategories;

internal sealed record ListCategoriesQuery : IQuery<IReadOnlyList<VehicleCategoryDto>>;
