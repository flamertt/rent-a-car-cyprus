using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Simulation.Application.Queries.GetSimulationStatus;

internal sealed record GetSimulationStatusQuery : IQuery<SimulationStatusDto>;
