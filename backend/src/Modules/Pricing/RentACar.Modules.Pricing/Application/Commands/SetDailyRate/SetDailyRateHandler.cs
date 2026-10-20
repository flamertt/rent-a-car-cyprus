using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Pricing.Application.Abstractions;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Commands.SetDailyRate;

internal sealed class SetDailyRateHandler(IPricingDbContext db) : ICommandHandler<SetDailyRateCommand, Unit>
{
    public async Task<Unit> HandleAsync(SetDailyRateCommand command, CancellationToken cancellationToken)
    {
        var plan = await db.RatePlans.FirstOrDefaultAsync(p => p.CategoryId == command.CategoryId, cancellationToken)
            ?? throw new NotFoundException("Tarife", command.CategoryId);
        plan.SetDailyRate(command.DailyRate);
        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
