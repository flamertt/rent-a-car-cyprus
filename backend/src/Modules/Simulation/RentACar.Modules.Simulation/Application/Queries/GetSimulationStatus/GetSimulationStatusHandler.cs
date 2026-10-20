using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RentACar.Modules.Simulation.Application.Abstractions;
using RentACar.Modules.Simulation.Application.Engine;
using RentACar.Modules.Simulation.Application.Options;
using RentACar.Modules.Simulation.Application.Services;
using RentACar.Modules.Simulation.Domain.Enums;
using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Simulation.Application.Queries.GetSimulationStatus;

internal sealed class GetSimulationStatusHandler(
    ISimulationDbContext db, SimulationRuntime runtime, IClock clock, RouteMemory routes, IOptions<SimulationOptions> options)
    : IQueryHandler<GetSimulationStatusQuery, SimulationStatusDto>
{
    public async Task<SimulationStatusDto> HandleAsync(GetSimulationStatusQuery query, CancellationToken cancellationToken)
    {
        var state = await db.State.AsNoTracking().SingleAsync(cancellationToken);
        var active = await db.Trips.CountAsync(t => t.State != TripState.Finished, cancellationToken);
        return new SimulationStatusDto(
            runtime.Running, runtime.Speed, runtime.CatchingUp, clock.UtcNow, active, state.ReservationsCreated, state.LostDemand,
            state.Pickups, state.Returns, runtime.LastError ?? state.LastError, options.Value.Routing.OsrmUrl,
            routes.ById.Values.Any(r => r.Provider == "osrm"));
    }
}
