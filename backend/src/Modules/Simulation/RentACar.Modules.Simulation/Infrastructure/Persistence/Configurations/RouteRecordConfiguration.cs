using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentACar.Modules.Simulation.Domain.Entities;
using RentACar.Shared.Infrastructure.Database;

namespace RentACar.Modules.Simulation.Infrastructure.Persistence.Configurations;

internal sealed class RouteRecordConfiguration : IEntityTypeConfiguration<RouteRecord>
{
    public void Configure(EntityTypeBuilder<RouteRecord> builder)
    {
        builder.ToTable("routes");
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => r.Key).IsUnique();
        builder.Property(r => r.Key).HasMaxLength(64);
        builder.Property(r => r.Provider).HasMaxLength(16);
        builder.Property(r => r.Path).HasColumnType(GeoColumnTypes.LineString);
    }
}
