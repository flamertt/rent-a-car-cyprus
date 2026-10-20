using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Branches.Application.Abstractions;
using RentACar.Modules.Branches.Domain.Entities;
using RentACar.Shared.Infrastructure.Database;

namespace RentACar.Modules.Branches.Infrastructure.Persistence;

internal sealed class BranchesDbContext(DbContextOptions<BranchesDbContext> options) : ModuleDbContext(options), IBranchesDbContext
{
    public const string SchemaName = "branches";

    protected override string Schema => SchemaName;

    public DbSet<Branch> Branches => Set<Branch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("postgis");
        base.OnModelCreating(modelBuilder);
    }
}
