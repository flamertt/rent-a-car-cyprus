using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RentACar.Modules.Reservations.Domain.Entities;

namespace RentACar.Modules.Reservations.Application.Abstractions;

internal interface IReservationsDbContext
{
    DbSet<Reservation> Reservations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Kayıt hatası, aynı araca örtüşen rezervasyonu engelleyen veritabanı kısıtından mı geldi?</summary>
    bool IsVehicleOverlap(DbUpdateException exception);
}
