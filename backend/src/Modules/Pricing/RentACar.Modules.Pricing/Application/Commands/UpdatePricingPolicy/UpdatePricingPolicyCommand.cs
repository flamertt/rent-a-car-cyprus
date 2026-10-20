using RentACar.Modules.Pricing.Application.Queries.GetPricingPolicy;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Commands.UpdatePricingPolicy;

internal sealed record UpdatePricingPolicyCommand(
    decimal OneWayFee, decimal OutOfHoursFee, int YoungDriverMaxAge, decimal YoungDriverDailyFee, int GraceMinutes)
    : ICommand<PricingPolicyDto>;
