using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Pricing.Application.Abstractions;
using RentACar.Modules.Pricing.Application.Mappings;
using RentACar.Modules.Pricing.Application.Queries.GetPricingPolicy;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Commands.UpdatePricingPolicy;

internal sealed class UpdatePricingPolicyHandler(IPricingDbContext db) : ICommandHandler<UpdatePricingPolicyCommand, PricingPolicyDto>
{
    public async Task<PricingPolicyDto> HandleAsync(UpdatePricingPolicyCommand c, CancellationToken cancellationToken)
    {
        var policy = await db.Policies.SingleAsync(cancellationToken);
        policy.Update(c.OneWayFee, c.OutOfHoursFee, c.YoungDriverMaxAge, c.YoungDriverDailyFee, c.GraceMinutes);
        await db.SaveChangesAsync(cancellationToken);
        return policy.ToDto();
    }
}
