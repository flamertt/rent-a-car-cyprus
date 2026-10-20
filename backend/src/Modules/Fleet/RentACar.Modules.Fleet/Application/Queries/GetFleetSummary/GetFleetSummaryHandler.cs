using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Fleet.Application.Abstractions;
using RentACar.Modules.Fleet.Contracts.Enums;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Fleet.Application.Queries.GetFleetSummary;

internal sealed class GetFleetSummaryHandler(IFleetDbContext db) : IQueryHandler<GetFleetSummaryQuery, FleetSummaryDto>
{
    public async Task<FleetSummaryDto> HandleAsync(GetFleetSummaryQuery query, CancellationToken cancellationToken)
    {
        var byStatus = await db.Vehicles
            .GroupBy(v => v.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

        var byCategory = await db.Vehicles
            .Where(v => v.Status != VehicleStatus.Retired)
            .GroupBy(v => new { v.Category.Code, v.Category.Name, v.Category.SortOrder })
            .OrderBy(g => g.Key.SortOrder)
            .Select(g => new CategoryCountDto(
                g.Key.Code,
                g.Key.Name,
                g.Count(),
                g.Count(v => v.Status == VehicleStatus.Available),
                g.Count(v => v.Status == VehicleStatus.OnRent)))
            .ToListAsync(cancellationToken);

        return new FleetSummaryDto(byStatus, byCategory);
    }
}
