using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Queries.GetPricingPolicy;

internal sealed record GetPricingPolicyQuery : IQuery<PricingPolicyDto>;
