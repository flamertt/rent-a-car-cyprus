using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RentACar.Modules.Simulation.Application.Commands.ControlSimulation;
using RentACar.Modules.Simulation.Application.Queries.GetSimulationStatus;
using RentACar.Modules.Simulation.Application.Queries.ListActiveTrips;
using RentACar.Modules.Simulation.Application.Queries.ListRecentEvents;
using RentACar.Modules.Simulation.Endpoints.Requests;
using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Infrastructure.Endpoints;

namespace RentACar.Modules.Simulation.Endpoints;

internal static class AdminSimulationEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapAdminGroup("simulation", "Simülasyon");
        group.MapGet("/", (IDispatcher d, CancellationToken ct) => d.QueryAsync(new GetSimulationStatusQuery(), ct));
        group.MapPut("/", (ControlSimulationRequest r, IDispatcher d, CancellationToken ct) =>
                d.SendAsync(new ControlSimulationCommand(r.Running, r.Speed), ct))
            .WithSummary("Başlat / duraklat / hız (simüle saniye ÷ gerçek saniye)");
        group.MapGet("/trips", (IDispatcher d, CancellationToken ct) => d.QueryAsync(new ListActiveTripsQuery(), ct))
            .WithSummary("Aktif yolculuklar ve rotaları");
        group.MapGet("/events", (int? limit, IDispatcher d, CancellationToken ct) => d.QueryAsync(new ListRecentEventsQuery(limit ?? 50), ct))
            .WithSummary("Son olaylar (rezervasyon, teslim, iade, kayıp talep…)");
    }
}
