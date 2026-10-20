using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Branches.Contracts;
using RentACar.Modules.Fleet.Contracts;
using RentACar.Modules.Pricing.Application.Abstractions;
using RentACar.Modules.Pricing.Application.Mappings;
using RentACar.Modules.Pricing.Contracts.Dtos;
using RentACar.Modules.Pricing.Domain.Services;
using RentACar.Modules.Pricing.Domain.ValueObjects;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Queries.CalculateQuotes;

internal sealed class CalculateQuotesHandler(IPricingDbContext db, IFleetApi fleet, IBranchesApi branches)
    : IQueryHandler<CalculateQuotesQuery, IReadOnlyDictionary<Guid, QuoteDto>>
{
    public async Task<IReadOnlyDictionary<Guid, QuoteDto>> HandleAsync(CalculateQuotesQuery query, CancellationToken cancellationToken)
    {
        var template = query.Template;
        var ids = query.CategoryIds.ToList();
        var categories = (await fleet.ListCategoriesAsync(cancellationToken)).Where(c => ids.Contains(c.Id)).ToList();
        if (categories.Count == 0)
        {
            throw new NotFoundException("Araç sınıfı", string.Join(",", ids));
        }

        var plans = await db.RatePlans.AsNoTracking()
            .Where(r => ids.Contains(r.CategoryId))
            .ToDictionaryAsync(r => r.CategoryId, r => r.DailyRate, cancellationToken);
        var pickup = await branches.GetAsync(template.PickupBranchId, cancellationToken)
            ?? throw new NotFoundException("Teslim şubesi", template.PickupBranchId);
        var ret = template.ReturnBranchId == template.PickupBranchId
            ? pickup
            : await branches.GetAsync(template.ReturnBranchId, cancellationToken) ?? throw new NotFoundException("İade şubesi", template.ReturnBranchId);

        // Sınıftan bağımsız girdiler bir kez yüklenir.
        var seasons = await db.Seasons.AsNoTracking().ToListAsync(cancellationToken);
        var discounts = await db.DurationDiscounts.AsNoTracking().ToListAsync(cancellationToken);
        var extras = await db.Extras.AsNoTracking().ToDictionaryAsync(e => e.Code, cancellationToken);
        var policy = await db.Policies.AsNoTracking().SingleAsync(cancellationToken);
        var pickupInfo = pickup.ToPricingInfo();
        var returnInfo = ret.ToPricingInfo();

        return categories
            .Where(c => plans.ContainsKey(c.Id))
            .ToDictionary(
                c => c.Id,
                c => PriceCalculator.Calculate(
                    template with { CategoryId = c.Id },
                    new PricingInputs(plans[c.Id], c.Deposit, seasons, discounts, extras, policy, pickupInfo, returnInfo)));
    }
}
