using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Branches.Application.Abstractions;
using RentACar.Modules.Branches.Application.Mappings;
using RentACar.Modules.Branches.Application.Services;
using RentACar.Modules.Branches.Contracts.Dtos;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Branches.Application.Commands.UpdateBranch;

internal sealed class UpdateBranchHandler(IBranchesDbContext db) : ICommandHandler<UpdateBranchCommand, BranchDto>
{
    public async Task<BranchDto> HandleAsync(UpdateBranchCommand c, CancellationToken cancellationToken)
    {
        var branch = await db.Branches.FirstOrDefaultAsync(b => b.Id == c.Id, cancellationToken)
            ?? throw new NotFoundException("Şube", c.Id);
        await BranchCodeGuard.EnsureFreeAsync(db, c.Code, c.Id, cancellationToken);
        branch.Update(c.Code, c.Name, c.Type, c.District, c.Address, c.Location, c.Phone, c.OpensAt, c.ClosesAt, c.DeliveryFee);
        await db.SaveChangesAsync(cancellationToken);
        return branch.ToDto();
    }
}
