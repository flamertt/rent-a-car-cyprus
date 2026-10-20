namespace RentACar.Modules.Pricing.Contracts.Requests;

public sealed record QuoteRequest(
    Guid CategoryId,
    DateTimeOffset PickupAt,
    DateTimeOffset ReturnAt,
    Guid PickupBranchId,
    Guid ReturnBranchId,
    IReadOnlyList<QuoteExtraRequest>? Extras,
    int? DriverAge);
