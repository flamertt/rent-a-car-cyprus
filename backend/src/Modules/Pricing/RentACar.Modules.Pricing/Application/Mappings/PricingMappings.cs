using RentACar.Modules.Branches.Contracts.Dtos;
using RentACar.Modules.Pricing.Application.Queries.GetPricingPolicy;
using RentACar.Modules.Pricing.Application.Queries.ListSeasons;
using RentACar.Modules.Pricing.Domain.Entities;
using RentACar.Modules.Pricing.Domain.ValueObjects;

namespace RentACar.Modules.Pricing.Application.Mappings;

internal static class PricingMappings
{
    public static BranchPricingInfo ToPricingInfo(this BranchDto b) =>
        new(b.Name, b.District.ToString(), b.DeliveryFee, b.OpensAt, b.ClosesAt);

    public static SeasonDto ToDto(this Season s) =>
        new(s.Id, s.Name, s.StartMonth, s.StartDay, s.EndMonth, s.EndDay, s.Multiplier);

    public static PricingPolicyDto ToDto(this PricingPolicy p) =>
        new(p.OneWayFee, p.OutOfHoursFee, p.YoungDriverMaxAge, p.YoungDriverDailyFee, p.GraceMinutes);
}
