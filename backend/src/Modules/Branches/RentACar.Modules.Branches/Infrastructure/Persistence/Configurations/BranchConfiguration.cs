using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentACar.Modules.Branches.Domain.Entities;
using RentACar.Shared.Infrastructure.Database;

namespace RentACar.Modules.Branches.Infrastructure.Persistence.Configurations;

internal sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.HasKey(b => b.Id);
        builder.HasIndex(b => b.Code).IsUnique();
        builder.Property(b => b.Code).HasMaxLength(16);
        builder.Property(b => b.Name).HasMaxLength(128);
        builder.Property(b => b.Address).HasMaxLength(512);
        builder.Property(b => b.Phone).HasMaxLength(32);
        builder.Property(b => b.Location).HasColumnType(GeoColumnTypes.Point);
        builder.HasIndex(b => b.Location).HasMethod("gist");
    }
}
