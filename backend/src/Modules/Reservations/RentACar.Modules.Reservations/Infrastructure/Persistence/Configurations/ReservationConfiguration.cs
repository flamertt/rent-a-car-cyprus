using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentACar.Modules.Reservations.Domain.Entities;

namespace RentACar.Modules.Reservations.Infrastructure.Persistence.Configurations;

internal sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => r.Code).IsUnique();
        builder.HasIndex(r => new { r.Status, r.PickupAt });
        builder.HasIndex(r => new { r.Status, r.ReturnAt });
        builder.HasIndex(r => r.VehicleId);
        builder.HasIndex(r => r.CustomerId);
        builder.Property(r => r.Code).HasMaxLength(12);
        builder.Property(r => r.CustomerName).HasMaxLength(130);
        builder.Property(r => r.CustomerEmail).HasMaxLength(254);
        builder.Property(r => r.CategoryCode).HasMaxLength(4);
        builder.Property(r => r.VehiclePlate).HasMaxLength(16);
        builder.Property(r => r.Currency).HasMaxLength(3).IsFixedLength();
        builder.OwnsMany(r => r.Lines, lines => lines.ToJson());
        builder.Property<uint>("Version").IsRowVersion();
    }
}
