using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using RentACar.Modules.Fleet.Application.Abstractions;
using RentACar.Modules.Fleet.Application.Commands.ChangeVehicleStatus;
using RentACar.Modules.Fleet.Application.Commands.CreateVehicle;
using RentACar.Modules.Fleet.Application.Dtos;
using RentACar.Modules.Fleet.Application.Queries.GetFleetGeoJson;
using RentACar.Modules.Fleet.Application.Queries.GetFleetSummary;
using RentACar.Modules.Fleet.Application.Queries.GetVehicle;
using RentACar.Modules.Fleet.Application.Queries.ListVehicles;
using RentACar.Modules.Fleet.Contracts.Dtos;
using RentACar.Modules.Fleet.Contracts.Enums;
using RentACar.Modules.Fleet.Endpoints.Requests;
using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Infrastructure.Endpoints;

namespace RentACar.Modules.Fleet.Endpoints;

internal static class AdminFleetEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapAdminGroup("fleet", "Filo");

        group.MapGet("/vehicles", (VehicleStatus? status, Guid? categoryId, Guid? branchId, string? search, int? page, int? pageSize,
                IDispatcher d, CancellationToken ct) =>
                d.QueryAsync(new ListVehiclesQuery(status, categoryId, branchId, search, page, pageSize), ct))
            .WithSummary("Araç listesi: durum/sınıf/şube filtresi, plaka-model araması, sayfalama");
        group.MapGet("/vehicles/{id:guid}", GetVehicleAsync);
        group.MapPost("/vehicles", CreateVehicleAsync);
        group.MapPut("/vehicles/{id:guid}/status", (Guid id, ChangeVehicleStatusRequest r, IDispatcher d, CancellationToken ct) =>
                d.SendAsync(new ChangeVehicleStatusCommand(id, r.Status), ct))
            .WithSummary("Bakıma al / bakımdan çıkar / filodan çıkar");
        group.MapGet("/summary", (IDispatcher d, CancellationToken ct) => d.QueryAsync(new GetFleetSummaryQuery(), ct))
            .WithSummary("Durum ve sınıf bazında sayılar");
        group.MapGet("/vehicles/geojson", (IDispatcher d, CancellationToken ct) => d.QueryAsync(new GetFleetGeoJsonQuery(), ct))
            .WithSummary("Tüm araçların anlık konumu (GeoJSON)");
        group.MapGet("/live", Live).WithSummary("Canlı konum akışı (Server-Sent Events, olay adı: positions)");
    }

    private static async Task<Results<Ok<VehicleDto>, NotFound>> GetVehicleAsync(Guid id, IDispatcher dispatcher, CancellationToken ct) =>
        await dispatcher.QueryAsync(new GetVehicleQuery(id), ct) is { } vehicle ? TypedResults.Ok(vehicle) : TypedResults.NotFound();

    private static async Task<Created<VehicleDto>> CreateVehicleAsync(CreateVehicleRequest r, IDispatcher dispatcher, CancellationToken ct)
    {
        var vehicle = await dispatcher.SendAsync(
            new CreateVehicleCommand(r.Plate, r.Make, r.Model, r.Year, r.Color, r.CategoryId, r.BranchId, r.OdometerKm), ct);
        return TypedResults.Created($"/api/admin/fleet/vehicles/{vehicle.Id}", vehicle);
    }

    private static ServerSentEventsResult<IReadOnlyList<LiveVehicleDto>> Live(IFleetLiveFeed live, CancellationToken ct) =>
        TypedResults.ServerSentEvents(FleetLiveStream.ReadAsync(live, ct), eventType: "positions");
}
