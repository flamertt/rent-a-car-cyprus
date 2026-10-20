namespace RentACar.Modules.Pricing.Endpoints.Requests;

internal sealed record UpdatePricingPolicyRequest(
    decimal OneWayFee, decimal OutOfHoursFee, int YoungDriverMaxAge, decimal YoungDriverDailyFee, int GraceMinutes);
