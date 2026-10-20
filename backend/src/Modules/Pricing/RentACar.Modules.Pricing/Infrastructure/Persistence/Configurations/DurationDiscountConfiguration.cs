using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentACar.Modules.Pricing.Domain.Entities;

namespace RentACar.Modules.Pricing.Infrastructure.Persistence.Configurations;

internal sealed class DurationDiscountConfiguration : IEntityTypeConfiguration<DurationDiscount>
{
    public void Configure(EntityTypeBuilder<DurationDiscount> builder)
    {
        builder.HasKey(d => d.Id);
        builder.HasIndex(d => d.MinDays).IsUnique();
        builder.Property(d => d.Percent).HasPrecision(5, 2);
    }
}
