using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Pricing.Application.Abstractions;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Commands.DeleteSeason;

internal sealed class DeleteSeasonHandler(IPricingDbContext db) : ICommandHandler<DeleteSeasonCommand, Unit>
{
    public async Task<Unit> HandleAsync(DeleteSeasonCommand command, CancellationToken cancellationToken) =>
        await db.Seasons.Where(s => s.Id == command.Id).ExecuteDeleteAsync(cancellationToken) == 0
            ? throw new NotFoundException("Sezon", command.Id)
            : Unit.Value;
}
