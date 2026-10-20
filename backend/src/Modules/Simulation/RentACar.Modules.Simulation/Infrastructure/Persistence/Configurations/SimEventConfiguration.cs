using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentACar.Modules.Simulation.Domain.Entities;

namespace RentACar.Modules.Simulation.Infrastructure.Persistence.Configurations;

internal sealed class SimEventConfiguration : IEntityTypeConfiguration<SimEvent>
{
    public void Configure(EntityTypeBuilder<SimEvent> builder)
    {
        builder.ToTable("events");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).UseIdentityAlwaysColumn();
        builder.HasIndex(e => e.At);
        builder.Property(e => e.Kind).HasMaxLength(32);
        builder.Property(e => e.Message).HasMaxLength(512);
        builder.Property(e => e.ReservationCode).HasMaxLength(12);
    }
}
