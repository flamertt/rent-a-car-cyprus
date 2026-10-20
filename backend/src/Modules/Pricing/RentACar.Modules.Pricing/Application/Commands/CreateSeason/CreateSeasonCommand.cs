using RentACar.Modules.Pricing.Application.Queries.ListSeasons;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Commands.CreateSeason;

internal sealed record CreateSeasonCommand(string Name, int StartMonth, int StartDay, int EndMonth, int EndDay, decimal Multiplier)
    : ICommand<SeasonDto>;
