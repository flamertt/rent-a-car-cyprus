using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Branches.Application.Abstractions;
using RentACar.Modules.Branches.Application.Mappings;
using RentACar.Modules.Branches.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Branches.Application.Queries.GetBranch;

internal sealed class GetBranchHandler(IBranchesDbContext db) : IQueryHandler<GetBranchQuery, BranchDto?>
{
    public async Task<BranchDto?> HandleAsync(GetBranchQuery query, CancellationToken cancellationToken)
    {
        var branch = await db.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == query.Id, cancellationToken);
        return branch?.ToDto();
    }
}
