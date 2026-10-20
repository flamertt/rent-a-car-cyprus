using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Pricing.Application.Abstractions;
using RentACar.Modules.Pricing.Domain.Entities;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Commands.UpdateExtra;

internal sealed class UpdateExtraHandler(IPricingDbContext db) : ICommandHandler<UpdateExtraCommand, Unit>
{
    public async Task<Unit> HandleAsync(UpdateExtraCommand c, CancellationToken cancellationToken)
    {
        var code = Extra.NormalizeCode(c.Code);
        var extra = await db.Extras.FirstOrDefaultAsync(e => e.Code == code, cancellationToken)
            ?? throw new NotFoundException("Ekstra", code);
        extra.Update(c.Name, c.Description, c.Price, c.MaxQuantity, c.MaxChargeDays, c.IsActive);
        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
