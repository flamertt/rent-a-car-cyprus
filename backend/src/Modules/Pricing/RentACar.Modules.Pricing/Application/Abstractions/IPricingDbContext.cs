using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Pricing.Domain.Entities;

namespace RentACar.Modules.Pricing.Application.Abstractions;

internal interface IPricingDbContext
{
    DbSet<RatePlan> RatePlans { get; }

    DbSet<Season> Seasons { get; }

    DbSet<DurationDiscount> DurationDiscounts { get; }

    DbSet<Extra> Extras { get; }

    DbSet<PricingPolicy> Policies { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
