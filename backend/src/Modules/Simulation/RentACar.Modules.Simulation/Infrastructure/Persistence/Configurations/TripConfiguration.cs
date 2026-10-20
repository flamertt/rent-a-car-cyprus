using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentACar.Modules.Simulation.Domain.Entities;

namespace RentACar.Modules.Simulation.Infrastructure.Persistence.Configurations;

internal sealed class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.HasKey(t => t.Id);
        builder.HasIndex(t => t.State);
        builder.HasIndex(t => new { t.VehicleId, t.State });
        builder.HasIndex(t => t.ReservationId);
        builder.Property(t => t.Plate).HasMaxLength(16);
        builder.Property(t => t.ReservationCode).HasMaxLength(12);
        builder.Property(t => t.CustomerName).HasMaxLength(130);
        builder.Property(t => t.DestinationName).HasMaxLength(128);
        builder.Property(t => t.StayName).HasMaxLength(128);
        builder.Property(t => t.LastAttraction).HasMaxLength(128);
        builder.Ignore(t => t.From);
        builder.Ignore(t => t.To);
        builder.Ignore(t => t.Stay);
    }
}
