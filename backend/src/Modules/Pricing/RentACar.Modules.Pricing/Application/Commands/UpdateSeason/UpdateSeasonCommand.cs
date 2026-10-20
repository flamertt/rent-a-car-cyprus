using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Commands.UpdateSeason;

internal sealed record UpdateSeasonCommand(Guid Id, string Name, int StartMonth, int StartDay, int EndMonth, int EndDay, decimal Multiplier)
    : ICommand<Unit>;
