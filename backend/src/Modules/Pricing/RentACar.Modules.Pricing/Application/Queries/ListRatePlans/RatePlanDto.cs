namespace RentACar.Modules.Pricing.Application.Queries.ListRatePlans;

internal sealed record RatePlanDto(Guid CategoryId, string CategoryCode, string CategoryName, decimal DailyRate, decimal HighSeasonDailyRate);
