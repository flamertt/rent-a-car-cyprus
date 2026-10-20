using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Queries.ListSeasons;

internal sealed record ListSeasonsQuery : IQuery<IReadOnlyList<SeasonDto>>;
