using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentACar.Modules.Auth.Domain.Entities;

namespace RentACar.Modules.Auth.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Email).HasMaxLength(254);
        builder.Property(u => u.DisplayName).HasMaxLength(64);
        builder.Property(u => u.PasswordHash).HasMaxLength(256);
    }
}
