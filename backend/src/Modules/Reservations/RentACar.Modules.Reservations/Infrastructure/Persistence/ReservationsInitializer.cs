using Microsoft.EntityFrameworkCore;
using RentACar.Shared.Abstractions.Modules;

namespace RentACar.Modules.Reservations.Infrastructure.Persistence;

internal sealed class ReservationsInitializer(ReservationsDbContext db) : IModuleInitializer
{
    public int Order => 50;

    public Task InitializeAsync(CancellationToken cancellationToken) => db.Database.MigrateAsync(cancellationToken);
}
