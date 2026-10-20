using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Simulation.Application.Abstractions;
using RentACar.Modules.Simulation.Application.Engine;
using RentACar.Modules.Simulation.Application.Queries.GetSimulationStatus;
using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Simulation.Application.Commands.ControlSimulation;

internal sealed class ControlSimulationHandler(ISimulationDbContext db, SimulationRuntime runtime, IClock clock, IDispatcher dispatcher)
    : ICommandHandler<ControlSimulationCommand, SimulationStatusDto>
{
    public async Task<SimulationStatusDto> HandleAsync(ControlSimulationCommand command, CancellationToken cancellationToken)
    {
        runtime.Apply(command.Running ?? runtime.Running, command.Speed ?? runtime.Speed);
        await db.State.ExecuteUpdateAsync(s => s
            .SetProperty(x => x.Running, runtime.Running)
            .SetProperty(x => x.Speed, runtime.Speed)
            .SetProperty(x => x.SimNow, clock.UtcNow), cancellationToken);
        return await dispatcher.QueryAsync(new GetSimulationStatusQuery(), cancellationToken);
    }
}
