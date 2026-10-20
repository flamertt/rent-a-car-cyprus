using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Fleet.Application.Abstractions;
using RentACar.Modules.Fleet.Application.Mappings;
using RentACar.Modules.Fleet.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Fleet.Application.Queries.GetCategory;

internal sealed class GetCategoryHandler(IFleetDbContext db) : IQueryHandler<GetCategoryQuery, VehicleCategoryDto?>
{
    public async Task<VehicleCategoryDto?> HandleAsync(GetCategoryQuery query, CancellationToken cancellationToken)
    {
        var category = await db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == query.Id, cancellationToken);
        return category?.ToDto();
    }
}
