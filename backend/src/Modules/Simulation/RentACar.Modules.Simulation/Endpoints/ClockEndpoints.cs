using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RentACar.Modules.Simulation.Application.Queries.GetClock;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Simulation.Endpoints;

internal static class ClockEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/api/clock", (IDispatcher d, CancellationToken ct) => d.QueryAsync(new GetClockQuery(), ct))
            .WithTags("Simülasyon")
            .WithSummary("Uygulama (simülasyon) saati ve hızı")
            .AllowAnonymous();
}
