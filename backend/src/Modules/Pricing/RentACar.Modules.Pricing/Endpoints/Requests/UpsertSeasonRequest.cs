namespace RentACar.Modules.Pricing.Endpoints.Requests;

internal sealed record UpsertSeasonRequest(string Name, int StartMonth, int StartDay, int EndMonth, int EndDay, decimal Multiplier);
