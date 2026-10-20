using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Branches.Application.Abstractions;
using RentACar.Modules.Branches.Application.Mappings;
using RentACar.Shared.Abstractions.Geo;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Branches.Application.Queries.FindNearestBranches;

internal sealed class FindNearestBranchesHandler(IBranchesDbContext db)
    : IQueryHandler<FindNearestBranchesQuery, IReadOnlyList<NearestBranchDto>>
{
    public async Task<IReadOnlyList<NearestBranchDto>> HandleAsync(FindNearestBranchesQuery query, CancellationToken cancellationToken)
    {
        var point = new GeoPoint(query.Lon, query.Lat).ToPoint();
        var branches = db.Branches.AsNoTracking().Where(b => b.IsActive);
        if (query.Type is { } type)
        {
            branches = branches.Where(b => b.Type == type);
        }

        // geography kolonunda Distance → ST_Distance (metre); sıralama gist indeksini kullanır.
        var rows = await branches
            .OrderBy(b => b.Location.Distance(point))
            .Take(Math.Clamp(query.Limit, 1, 20))
            .Select(b => new { Branch = b, Meters = b.Location.Distance(point) })
            .ToListAsync(cancellationToken);

        return rows.Select(r => new NearestBranchDto(r.Branch.ToDto(), Math.Round(r.Meters / 1000, 2))).ToList();
    }
}
