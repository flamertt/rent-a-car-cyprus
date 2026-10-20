using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Pricing.Application.Abstractions;
using RentACar.Modules.Pricing.Application.Mappings;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Queries.GetPricingPolicy;

internal sealed class GetPricingPolicyHandler(IPricingDbContext db) : IQueryHandler<GetPricingPolicyQuery, PricingPolicyDto>
{
    public async Task<PricingPolicyDto> HandleAsync(GetPricingPolicyQuery query, CancellationToken cancellationToken) =>
        (await db.Policies.AsNoTracking().SingleAsync(cancellationToken)).ToDto();
}
