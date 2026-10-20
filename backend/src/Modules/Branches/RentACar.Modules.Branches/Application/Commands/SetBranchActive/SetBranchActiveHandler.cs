using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Branches.Application.Abstractions;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Branches.Application.Commands.SetBranchActive;

internal sealed class SetBranchActiveHandler(IBranchesDbContext db) : ICommandHandler<SetBranchActiveCommand, Unit>
{
    public async Task<Unit> HandleAsync(SetBranchActiveCommand command, CancellationToken cancellationToken)
    {
        var updated = await db.Branches
            .Where(b => b.Id == command.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(b => b.IsActive, command.IsActive), cancellationToken);
        return updated == 0 ? throw new NotFoundException("Şube", command.Id) : Unit.Value;
    }
}
