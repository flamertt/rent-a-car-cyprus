using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Fleet.Application.Abstractions;
using RentACar.Modules.Fleet.Application.Mappings;
using RentACar.Modules.Fleet.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Fleet.Application.Queries.ListCategories;

internal sealed class ListCategoriesHandler(IFleetDbContext db) : IQueryHandler<ListCategoriesQuery, IReadOnlyList<VehicleCategoryDto>>
{
    public async Task<IReadOnlyList<VehicleCategoryDto>> HandleAsync(ListCategoriesQuery query, CancellationToken cancellationToken)
    {
        var categories = await db.Categories.AsNoTracking().OrderBy(c => c.SortOrder).ToListAsync(cancellationToken);
        return categories.Select(c => c.ToDto()).ToList();
    }
}
