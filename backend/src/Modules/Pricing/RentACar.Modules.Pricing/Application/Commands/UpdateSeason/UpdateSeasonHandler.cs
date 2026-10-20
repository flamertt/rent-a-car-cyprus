using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Pricing.Application.Abstractions;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Commands.UpdateSeason;

internal sealed class UpdateSeasonHandler(IPricingDbContext db) : ICommandHandler<UpdateSeasonCommand, Unit>
{
    public async Task<Unit> HandleAsync(UpdateSeasonCommand c, CancellationToken cancellationToken)
    {
        var season = await db.Seasons.FirstOrDefaultAsync(s => s.Id == c.Id, cancellationToken)
            ?? throw new NotFoundException("Sezon", c.Id);
        season.Update(c.Name, c.StartMonth, c.StartDay, c.EndMonth, c.EndDay, c.Multiplier);
        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
