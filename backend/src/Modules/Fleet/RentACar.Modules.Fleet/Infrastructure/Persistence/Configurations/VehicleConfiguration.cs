using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentACar.Modules.Fleet.Domain.Entities;
using RentACar.Shared.Infrastructure.Database;

namespace RentACar.Modules.Fleet.Infrastructure.Persistence.Configurations;

internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.HasKey(v => v.Id);
        builder.HasIndex(v => v.Plate).IsUnique();
        // Yer numarası şube içinde anlamlı (bkz. ParkingLot); şubedeki boş yer bu indeksle bulunur.
        builder.HasIndex(v => new { v.BranchId, v.ParkingSlot });
        builder.HasIndex(v => new { v.CategoryId, v.Status });
        builder.Property(v => v.Plate).HasMaxLength(16);
        builder.Property(v => v.Make).HasMaxLength(32);
        builder.Property(v => v.Model).HasMaxLength(48);
        builder.Property(v => v.Color).HasMaxLength(24);
        builder.Property(v => v.Position).HasColumnType(GeoColumnTypes.Point);
        builder.HasIndex(v => v.Position).HasMethod("gist");
        builder.HasOne(v => v.Category).WithMany().HasForeignKey(v => v.CategoryId).OnDelete(DeleteBehavior.Restrict);

        // İyimser eşzamanlılık: PostgreSQL xmin sistem kolonu (durum geçişleri çakışırsa DbUpdateConcurrencyException).
        builder.Property<uint>("Version").IsRowVersion();
    }
}
