using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Pricing.Application.Abstractions;
using RentACar.Modules.Pricing.Domain.Entities;
using RentACar.Shared.Infrastructure.Database;

namespace RentACar.Modules.Pricing.Infrastructure.Persistence;

internal sealed class PricingDbContext(DbContextOptions<PricingDbContext> options) : ModuleDbContext(options), IPricingDbContext
{
    public const string SchemaName = "pricing";

    protected override string Schema => SchemaName;

    public DbSet<RatePlan> RatePlans => Set<RatePlan>();

    public DbSet<Season> Seasons => Set<Season>();

    public DbSet<DurationDiscount> DurationDiscounts => Set<DurationDiscount>();

    public DbSet<Extra> Extras => Set<Extra>();

    public DbSet<PricingPolicy> Policies => Set<PricingPolicy>();
}
