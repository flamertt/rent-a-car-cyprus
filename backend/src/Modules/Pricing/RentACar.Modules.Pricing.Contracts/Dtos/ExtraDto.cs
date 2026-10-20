using RentACar.Modules.Pricing.Contracts.Enums;

namespace RentACar.Modules.Pricing.Contracts.Dtos;

public sealed record ExtraDto(
    string Code,
    string Name,
    string Description,
    ExtraUnit Unit,
    decimal Price,
    int MaxQuantity,
    int? MaxChargeDays);
