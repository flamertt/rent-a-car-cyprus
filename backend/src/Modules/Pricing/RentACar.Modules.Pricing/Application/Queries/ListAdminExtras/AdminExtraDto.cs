using RentACar.Modules.Pricing.Contracts.Enums;

namespace RentACar.Modules.Pricing.Application.Queries.ListAdminExtras;

internal sealed record AdminExtraDto(
    string Code, string Name, string Description, ExtraUnit Unit, decimal Price, int MaxQuantity, int? MaxChargeDays, bool IsActive);
