using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Pricing.Application.Abstractions;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Queries.ListAdminExtras;

internal sealed class ListAdminExtrasHandler(IPricingDbContext db) : IQueryHandler<ListAdminExtrasQuery, IReadOnlyList<AdminExtraDto>>
{
    public async Task<IReadOnlyList<AdminExtraDto>> HandleAsync(ListAdminExtrasQuery query, CancellationToken cancellationToken) =>
        await db.Extras.AsNoTracking()
            .OrderBy(e => e.Code)
            .Select(e => new AdminExtraDto(e.Code, e.Name, e.Description, e.Unit, e.Price, e.MaxQuantity, e.MaxChargeDays, e.IsActive))
            .ToListAsync(cancellationToken);
}
