namespace RentACar.Modules.Pricing.Endpoints.Requests;

internal sealed record UpdateExtraRequest(string Name, string Description, decimal Price, int MaxQuantity, int? MaxChargeDays, bool IsActive);
