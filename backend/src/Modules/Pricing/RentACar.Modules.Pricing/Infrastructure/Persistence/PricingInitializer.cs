using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Fleet.Contracts;
using RentACar.Modules.Pricing.Domain.Entities;
using RentACar.Modules.Pricing.Infrastructure.Persistence.Seed;
using RentACar.Shared.Abstractions.Modules;

namespace RentACar.Modules.Pricing.Infrastructure.Persistence;

internal sealed class PricingInitializer(PricingDbContext db, IFleetApi fleet) : IModuleInitializer
{
    public int Order => 30;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await db.Database.MigrateAsync(cancellationToken);
        if (await db.Policies.AnyAsync(cancellationToken))
        {
            return;
        }

        foreach (var category in await fleet.ListCategoriesAsync(cancellationToken))
        {
            db.RatePlans.Add(RatePlan.Create(
                category.Id, category.Code, PricingSeedData.BaseRates.GetValueOrDefault(category.Code, PricingSeedData.DefaultDailyRate)));
        }

        db.Seasons.AddRange(PricingSeedData.Seasons());
        db.DurationDiscounts.AddRange(PricingSeedData.Discounts());
        db.Extras.AddRange(PricingSeedData.Extras());
        db.Policies.Add(new PricingPolicy());
        await db.SaveChangesAsync(cancellationToken);
    }
}
