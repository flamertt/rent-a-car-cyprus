using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using RentACar.Modules.Reservations.Application.Abstractions;
using RentACar.Modules.Reservations.Domain.Entities;
using RentACar.Shared.Infrastructure.Database;

namespace RentACar.Modules.Reservations.Infrastructure.Persistence;

internal sealed class ReservationsDbContext(DbContextOptions<ReservationsDbContext> options) : ModuleDbContext(options), IReservationsDbContext
{
    public const string SchemaName = "reservations";

    /// <summary>Aynı araca örtüşen iki aktif rezervasyonu veritabanı düzeyinde engeller (Initial migration'da).</summary>
    public const string NoOverlapConstraint = "ex_reservations_vehicle_no_overlap";

    protected override string Schema => SchemaName;

    public DbSet<Reservation> Reservations => Set<Reservation>();

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        Database.BeginTransactionAsync(cancellationToken);

    public bool IsVehicleOverlap(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.ExclusionViolation, ConstraintName: NoOverlapConstraint };

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("btree_gist");
        base.OnModelCreating(modelBuilder);
    }
}
