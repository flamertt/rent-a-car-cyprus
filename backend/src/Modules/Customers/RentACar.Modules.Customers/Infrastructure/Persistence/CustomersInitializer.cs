using Microsoft.EntityFrameworkCore;
using RentACar.Shared.Abstractions.Modules;

namespace RentACar.Modules.Customers.Infrastructure.Persistence;

internal sealed class CustomersInitializer(CustomersDbContext db) : IModuleInitializer
{
    public int Order => 40;

    public Task InitializeAsync(CancellationToken cancellationToken) => db.Database.MigrateAsync(cancellationToken);
}
