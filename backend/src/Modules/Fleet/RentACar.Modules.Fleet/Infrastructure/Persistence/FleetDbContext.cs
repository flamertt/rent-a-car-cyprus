using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Fleet.Application.Abstractions;
using RentACar.Modules.Fleet.Domain.Entities;
using RentACar.Shared.Infrastructure.Database;

namespace RentACar.Modules.Fleet.Infrastructure.Persistence;

internal sealed class FleetDbContext(DbContextOptions<FleetDbContext> options) : ModuleDbContext(options), IFleetDbContext
{
    public const string SchemaName = "fleet";

    protected override string Schema => SchemaName;

    public DbSet<VehicleCategory> Categories => Set<VehicleCategory>();

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("postgis");
        base.OnModelCreating(modelBuilder);
    }
}
