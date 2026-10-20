namespace RentACar.Modules.Pricing.Application.Queries.GetPricingPolicy;

internal sealed record PricingPolicyDto(decimal OneWayFee, decimal OutOfHoursFee, int YoungDriverMaxAge, decimal YoungDriverDailyFee, int GraceMinutes);
