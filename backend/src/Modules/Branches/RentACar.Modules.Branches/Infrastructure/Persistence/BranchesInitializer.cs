using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Branches.Infrastructure.Persistence.Seed;
using RentACar.Shared.Abstractions.Modules;

namespace RentACar.Modules.Branches.Infrastructure.Persistence;

internal sealed class BranchesInitializer(BranchesDbContext db) : IModuleInitializer
{
    public int Order => 10;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await db.Database.MigrateAsync(cancellationToken);
        if (await db.Branches.AnyAsync(cancellationToken))
        {
            return;
        }

        db.Branches.AddRange(BranchSeedData.Create());
        await db.SaveChangesAsync(cancellationToken);
    }
}
