using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Pricing.Application.Abstractions;
using RentACar.Modules.Pricing.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Queries.ListExtras;

internal sealed class ListExtrasHandler(IPricingDbContext db) : IQueryHandler<ListExtrasQuery, IReadOnlyList<ExtraDto>>
{
    public async Task<IReadOnlyList<ExtraDto>> HandleAsync(ListExtrasQuery query, CancellationToken cancellationToken) =>
        await db.Extras.AsNoTracking()
            .Where(e => e.IsActive)
            .OrderBy(e => e.Unit).ThenBy(e => e.Price)
            .Select(e => new ExtraDto(e.Code, e.Name, e.Description, e.Unit, e.Price, e.MaxQuantity, e.MaxChargeDays))
            .ToListAsync(cancellationToken);
}
