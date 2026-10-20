using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentACar.Modules.Simulation.Domain.Entities;

namespace RentACar.Modules.Simulation.Infrastructure.Persistence.Configurations;

internal sealed class SimStateConfiguration : IEntityTypeConfiguration<SimState>
{
    public void Configure(EntityTypeBuilder<SimState> builder)
    {
        builder.ToTable("state");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.LastError).HasMaxLength(1024);
    }
}
