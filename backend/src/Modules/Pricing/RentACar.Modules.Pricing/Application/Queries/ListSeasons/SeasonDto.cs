namespace RentACar.Modules.Pricing.Application.Queries.ListSeasons;

internal sealed record SeasonDto(Guid Id, string Name, int StartMonth, int StartDay, int EndMonth, int EndDay, decimal Multiplier);
