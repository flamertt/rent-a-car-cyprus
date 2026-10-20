using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Pricing.Application.Abstractions;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Queries.ListSeasons;

internal sealed class ListSeasonsHandler(IPricingDbContext db) : IQueryHandler<ListSeasonsQuery, IReadOnlyList<SeasonDto>>
{
    public async Task<IReadOnlyList<SeasonDto>> HandleAsync(ListSeasonsQuery query, CancellationToken cancellationToken) =>
        await db.Seasons.AsNoTracking()
            .OrderBy(s => s.StartMonth).ThenBy(s => s.StartDay)
            .Select(s => new SeasonDto(s.Id, s.Name, s.StartMonth, s.StartDay, s.EndMonth, s.EndDay, s.Multiplier))
            .ToListAsync(cancellationToken);
}
