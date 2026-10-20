using RentACar.Modules.Branches.Application.Abstractions;
using RentACar.Modules.Branches.Application.Mappings;
using RentACar.Modules.Branches.Application.Services;
using RentACar.Modules.Branches.Contracts.Dtos;
using RentACar.Modules.Branches.Domain.Entities;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Branches.Application.Commands.CreateBranch;

internal sealed class CreateBranchHandler(IBranchesDbContext db) : ICommandHandler<CreateBranchCommand, BranchDto>
{
    public async Task<BranchDto> HandleAsync(CreateBranchCommand c, CancellationToken cancellationToken)
    {
        await BranchCodeGuard.EnsureFreeAsync(db, c.Code, null, cancellationToken);
        var branch = Branch.Create(c.Code, c.Name, c.Type, c.District, c.Address, c.Location, c.Phone, c.OpensAt, c.ClosesAt, c.DeliveryFee);
        db.Branches.Add(branch);
        await db.SaveChangesAsync(cancellationToken);
        return branch.ToDto();
    }
}
