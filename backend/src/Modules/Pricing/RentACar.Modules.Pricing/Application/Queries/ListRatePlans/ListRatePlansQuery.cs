using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Queries.ListRatePlans;

internal sealed record ListRatePlansQuery : IQuery<IReadOnlyList<RatePlanDto>>;
