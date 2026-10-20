namespace RentACar.Modules.Pricing.Contracts.Dtos;

public sealed record QuoteLineDto(string Code, string Description, int Quantity, decimal UnitPrice, decimal Amount);
