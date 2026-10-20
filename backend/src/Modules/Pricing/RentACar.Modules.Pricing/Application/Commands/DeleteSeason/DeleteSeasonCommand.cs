using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Commands.DeleteSeason;

internal sealed record DeleteSeasonCommand(Guid Id) : ICommand<Unit>;
