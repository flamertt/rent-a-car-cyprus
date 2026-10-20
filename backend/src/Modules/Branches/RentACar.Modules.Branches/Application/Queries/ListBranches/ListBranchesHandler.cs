using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Branches.Application.Abstractions;
using RentACar.Modules.Branches.Application.Mappings;
using RentACar.Modules.Branches.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Branches.Application.Queries.ListBranches;

internal sealed class ListBranchesHandler(IBranchesDbContext db) : IQueryHandler<ListBranchesQuery, IReadOnlyList<BranchDto>>
{
    public async Task<IReadOnlyList<BranchDto>> HandleAsync(ListBranchesQuery query, CancellationToken cancellationToken)
    {
        var branches = db.Branches.AsNoTracking();
        if (query.OnlyActive)
        {
            branches = branches.Where(b => b.IsActive);
        }

        if (query.Type is { } type)
        {
            branches = branches.Where(b => b.Type == type);
        }

        if (query.District is { } district)
        {
            branches = branches.Where(b => b.District == district);
        }

        var ordered = query.OnlyActive
            ? branches.OrderBy(b => b.Type).ThenBy(b => b.Name)
            : branches.OrderBy(b => b.Code);
        var rows = await ordered.ToListAsync(cancellationToken);
        return rows.Select(b => b.ToDto()).ToList();
    }
}
