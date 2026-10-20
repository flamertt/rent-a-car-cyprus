using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using RentACar.Modules.Branches.Application.Queries.FindNearestBranches;
using RentACar.Modules.Branches.Application.Queries.GetBranch;
using RentACar.Modules.Branches.Application.Queries.GetBranchesGeoJson;
using RentACar.Modules.Branches.Application.Queries.ListBranches;
using RentACar.Modules.Branches.Contracts.Dtos;
using RentACar.Modules.Branches.Contracts.Enums;
using RentACar.Shared.Abstractions.Geo;
using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Infrastructure.Endpoints;

namespace RentACar.Modules.Branches.Endpoints;

internal static class PublicBranchEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapPublicGroup("branches", "Şubeler");

        group.MapGet("/", (BranchType? type, District? district, IDispatcher d, CancellationToken ct) =>
                d.QueryAsync(new ListBranchesQuery(OnlyActive: true, type, district), ct))
            .WithSummary("Aktif şube ve teslim noktaları");

        group.MapGet("/nearest", (double lon, double lat, BranchType? type, int? limit, IDispatcher d, CancellationToken ct) =>
                d.QueryAsync(new FindNearestBranchesQuery(lon, lat, type, limit ?? 3), ct))
            .WithSummary("Bir noktaya en yakın şubeler (PostGIS, metre)");

        group.MapGet("/geojson", (IDispatcher d, CancellationToken ct) => d.QueryAsync(new GetBranchesGeoJsonQuery(), ct))
            .WithSummary("Harita için GeoJSON FeatureCollection");

        group.MapGet("/{id:guid}", GetAsync);
    }

    private static async Task<Results<Ok<BranchDto>, NotFound>> GetAsync(Guid id, IDispatcher dispatcher, CancellationToken ct) =>
        await dispatcher.QueryAsync(new GetBranchQuery(id), ct) is { } branch ? TypedResults.Ok(branch) : TypedResults.NotFound();
}
