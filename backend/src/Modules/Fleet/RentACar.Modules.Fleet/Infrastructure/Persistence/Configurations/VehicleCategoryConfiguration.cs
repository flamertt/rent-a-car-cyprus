using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentACar.Modules.Fleet.Domain.Entities;

namespace RentACar.Modules.Fleet.Infrastructure.Persistence.Configurations;

internal sealed class VehicleCategoryConfiguration : IEntityTypeConfiguration<VehicleCategory>
{
    public void Configure(EntityTypeBuilder<VehicleCategory> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.Code).IsUnique();
        builder.Property(c => c.Code).HasMaxLength(4);
        builder.Property(c => c.Name).HasMaxLength(64);
    }
}
