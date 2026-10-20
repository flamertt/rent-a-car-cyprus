using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentACar.Modules.Pricing.Domain.Entities;

namespace RentACar.Modules.Pricing.Infrastructure.Persistence.Configurations;

internal sealed class RatePlanConfiguration : IEntityTypeConfiguration<RatePlan>
{
    public void Configure(EntityTypeBuilder<RatePlan> builder)
    {
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => r.CategoryId).IsUnique();
        builder.Property(r => r.CategoryCode).HasMaxLength(4);
    }
}
