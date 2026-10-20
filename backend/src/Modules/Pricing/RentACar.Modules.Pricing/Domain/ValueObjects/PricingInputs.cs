using RentACar.Modules.Pricing.Domain.Entities;

namespace RentACar.Modules.Pricing.Domain.ValueObjects;

internal sealed record PricingInputs(
    decimal BaseDailyRate,
    decimal Deposit,
    IReadOnlyList<Season> Seasons,
    IReadOnlyList<DurationDiscount> Discounts,
    IReadOnlyDictionary<string, Extra> Extras,
    PricingPolicy Policy,
    BranchPricingInfo PickupBranch,
    BranchPricingInfo ReturnBranch);
