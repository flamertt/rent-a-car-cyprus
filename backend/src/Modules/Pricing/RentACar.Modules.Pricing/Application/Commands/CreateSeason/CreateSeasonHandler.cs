using RentACar.Modules.Pricing.Application.Abstractions;
using RentACar.Modules.Pricing.Application.Mappings;
using RentACar.Modules.Pricing.Application.Queries.ListSeasons;
using RentACar.Modules.Pricing.Domain.Entities;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Commands.CreateSeason;

internal sealed class CreateSeasonHandler(IPricingDbContext db) : ICommandHandler<CreateSeasonCommand, SeasonDto>
{
    public async Task<SeasonDto> HandleAsync(CreateSeasonCommand c, CancellationToken cancellationToken)
    {
        var season = Season.Create(c.Name, c.StartMonth, c.StartDay, c.EndMonth, c.EndDay, c.Multiplier);
        db.Seasons.Add(season);
        await db.SaveChangesAsync(cancellationToken);
        return season.ToDto();
    }
}
