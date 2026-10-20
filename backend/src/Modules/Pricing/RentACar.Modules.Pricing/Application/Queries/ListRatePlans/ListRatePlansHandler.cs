using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Fleet.Contracts;
using RentACar.Modules.Pricing.Application.Abstractions;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Queries.ListRatePlans;

internal sealed class ListRatePlansHandler(IPricingDbContext db, IFleetApi fleet) : IQueryHandler<ListRatePlansQuery, IReadOnlyList<RatePlanDto>>
{
    public async Task<IReadOnlyList<RatePlanDto>> HandleAsync(ListRatePlansQuery query, CancellationToken cancellationToken)
    {
        var plans = await db.RatePlans.AsNoTracking().ToListAsync(cancellationToken);
        var seasons = await db.Seasons.AsNoTracking().ToListAsync(cancellationToken);
        var peak = seasons.Select(s => s.Multiplier).DefaultIfEmpty(1m).Max();
        var categories = await fleet.ListCategoriesAsync(cancellationToken);

        // Modüller arası "join": iki ayrı şema, bellekte birleştirilir (veri küçük).
        return (from c in categories
                join p in plans on c.Id equals p.CategoryId
                orderby c.SortOrder
                select new RatePlanDto(c.Id, c.Code, c.Name, p.DailyRate, Math.Round(p.DailyRate * peak)))
            .ToList();
    }
}
