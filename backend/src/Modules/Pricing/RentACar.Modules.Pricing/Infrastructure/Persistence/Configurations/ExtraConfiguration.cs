using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentACar.Modules.Pricing.Domain.Entities;

namespace RentACar.Modules.Pricing.Infrastructure.Persistence.Configurations;

internal sealed class ExtraConfiguration : IEntityTypeConfiguration<Extra>
{
    public void Configure(EntityTypeBuilder<Extra> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Code).IsUnique();
        builder.Property(e => e.Code).HasMaxLength(32);
        builder.Property(e => e.Name).HasMaxLength(96);
        builder.Property(e => e.Description).HasMaxLength(512);
    }
}
