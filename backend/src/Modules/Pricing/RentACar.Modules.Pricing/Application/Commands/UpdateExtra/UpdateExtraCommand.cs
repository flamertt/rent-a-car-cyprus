using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Commands.UpdateExtra;

internal sealed record UpdateExtraCommand(
    string Code, string Name, string Description, decimal Price, int MaxQuantity, int? MaxChargeDays, bool IsActive) : ICommand<Unit>;
