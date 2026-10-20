using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Fleet.Application.Abstractions;
using RentACar.Modules.Fleet.Application.Mappings;
using RentACar.Modules.Fleet.Contracts.Dtos;
using RentACar.Shared.Abstractions.Common;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Fleet.Application.Queries.ListVehicles;

internal sealed class ListVehiclesHandler(IFleetDbContext db) : IQueryHandler<ListVehiclesQuery, PagedResult<VehicleDto>>
{
    public async Task<PagedResult<VehicleDto>> HandleAsync(ListVehiclesQuery q, CancellationToken cancellationToken)
    {
        var (page, size) = Paging.Normalize(q.Page, q.PageSize);
        var vehicles = db.Vehicles.AsNoTracking();
        if (q.Status is { } status)
        {
            vehicles = vehicles.Where(v => v.Status == status);
        }

        if (q.CategoryId is { } categoryId)
        {
            vehicles = vehicles.Where(v => v.CategoryId == categoryId);
        }

        if (q.BranchId is { } branchId)
        {
            vehicles = vehicles.Where(v => v.BranchId == branchId);
        }

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var pattern = $"%{q.Search.Trim()}%";
            vehicles = vehicles.Where(v => EF.Functions.ILike(v.Plate, pattern) || EF.Functions.ILike(v.Make + " " + v.Model, pattern));
        }

        var total = await vehicles.CountAsync(cancellationToken);
        var items = await vehicles
            .Include(v => v.Category)
            .OrderBy(v => v.Category.SortOrder).ThenBy(v => v.Plate)
            .Skip((page - 1) * size).Take(size)
            .ToListAsync(cancellationToken);
        return new PagedResult<VehicleDto>(items.Select(v => v.ToDto()).ToList(), page, size, total);
    }
}
