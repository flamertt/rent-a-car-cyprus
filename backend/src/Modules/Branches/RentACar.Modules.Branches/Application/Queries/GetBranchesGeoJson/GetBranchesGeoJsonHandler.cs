using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Branches.Application.Abstractions;
using RentACar.Shared.Abstractions.Geo;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Branches.Application.Queries.GetBranchesGeoJson;

internal sealed class GetBranchesGeoJsonHandler(IBranchesDbContext db) : IQueryHandler<GetBranchesGeoJsonQuery, GeoJsonFeatureCollection>
{
    public async Task<GeoJsonFeatureCollection> HandleAsync(GetBranchesGeoJsonQuery query, CancellationToken cancellationToken)
    {
        var branches = await db.Branches.AsNoTracking().Where(b => b.IsActive).ToListAsync(cancellationToken);
        return new GeoJsonFeatureCollection(branches
            .Select(b => new GeoJsonFeature(
                b.Id.ToString(),
                GeoJsonPoint.From(b.Location.ToGeoPoint()),
                new Dictionary<string, object?>
                {
                    ["code"] = b.Code,
                    ["name"] = b.Name,
                    ["type"] = b.Type.ToString(),
                    ["district"] = b.District.ToString(),
                    ["deliveryFee"] = b.DeliveryFee,
                }))
            .ToList());
    }
}
