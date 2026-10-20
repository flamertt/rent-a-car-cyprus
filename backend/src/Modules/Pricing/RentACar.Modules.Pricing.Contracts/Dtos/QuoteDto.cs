namespace RentACar.Modules.Pricing.Contracts.Dtos;

public sealed record QuoteDto(
    Guid CategoryId,
    int Days,
    string Currency,
    IReadOnlyList<QuoteLineDto> Lines,
    decimal Total,
    decimal Deposit);
