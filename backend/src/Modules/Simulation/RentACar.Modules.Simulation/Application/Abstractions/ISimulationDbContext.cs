using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Simulation.Domain.Entities;

namespace RentACar.Modules.Simulation.Application.Abstractions;

internal interface ISimulationDbContext
{
    DbSet<SimState> State { get; }

    DbSet<Trip> Trips { get; }

    DbSet<RouteRecord> Routes { get; }

    DbSet<SimEvent> Events { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
