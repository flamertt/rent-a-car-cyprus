using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using RentACar.Modules.Branches.Application.Commands.CreateBranch;
using RentACar.Modules.Branches.Application.Commands.SetBranchActive;
using RentACar.Modules.Branches.Application.Commands.UpdateBranch;
using RentACar.Modules.Branches.Application.Queries.ListBranches;
using RentACar.Modules.Branches.Contracts.Dtos;
using RentACar.Modules.Branches.Endpoints.Requests;
using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Infrastructure.Endpoints;

namespace RentACar.Modules.Branches.Endpoints;

internal static class AdminBranchEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapAdminGroup("branches", "Şubeler");

        group.MapGet("/", (IDispatcher d, CancellationToken ct) => d.QueryAsync(new ListBranchesQuery(OnlyActive: false), ct))
            .WithSummary("Pasifler dahil tüm şubeler");
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:guid}", (Guid id, UpsertBranchRequest r, IDispatcher d, CancellationToken ct) =>
            d.SendAsync(new UpdateBranchCommand(id, r.Code, r.Name, r.Type, r.District, r.Address, r.Location, r.Phone, r.OpensAt, r.ClosesAt, r.DeliveryFee), ct));
        group.MapPut("/{id:guid}/active", SetActiveAsync).WithSummary("Şubeyi aç/kapat");
    }

    private static async Task<Created<BranchDto>> CreateAsync(UpsertBranchRequest r, IDispatcher dispatcher, CancellationToken ct)
    {
        var branch = await dispatcher.SendAsync(
            new CreateBranchCommand(r.Code, r.Name, r.Type, r.District, r.Address, r.Location, r.Phone, r.OpensAt, r.ClosesAt, r.DeliveryFee), ct);
        return TypedResults.Created($"/api/branches/{branch.Id}", branch);
    }

    private static async Task<NoContent> SetActiveAsync(Guid id, bool isActive, IDispatcher dispatcher, CancellationToken ct)
    {
        await dispatcher.SendAsync(new SetBranchActiveCommand(id, isActive), ct);
        return TypedResults.NoContent();
    }
}
