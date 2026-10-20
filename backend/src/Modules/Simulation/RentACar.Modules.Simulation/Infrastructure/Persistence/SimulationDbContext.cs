using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Simulation.Application.Abstractions;
using RentACar.Modules.Simulation.Domain.Entities;
using RentACar.Shared.Infrastructure.Database;

namespace RentACar.Modules.Simulation.Infrastructure.Persistence;

internal sealed class SimulationDbContext(DbContextOptions<SimulationDbContext> options) : ModuleDbContext(options), ISimulationDbContext
{
    public const string SchemaName = "simulation";

    protected override string Schema => SchemaName;

    public DbSet<SimState> State => Set<SimState>();

    public DbSet<Trip> Trips => Set<Trip>();

    public DbSet<RouteRecord> Routes => Set<RouteRecord>();

    public DbSet<SimEvent> Events => Set<SimEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("postgis");
        base.OnModelCreating(modelBuilder);
    }
}
