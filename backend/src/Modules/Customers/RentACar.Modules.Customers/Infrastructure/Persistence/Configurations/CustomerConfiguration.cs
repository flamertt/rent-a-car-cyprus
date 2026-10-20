using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentACar.Modules.Customers.Domain.Entities;

namespace RentACar.Modules.Customers.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.Email).IsUnique();
        builder.HasIndex(c => new { c.LastName, c.FirstName });
        builder.Property(c => c.FirstName).HasMaxLength(64);
        builder.Property(c => c.LastName).HasMaxLength(64);
        builder.Property(c => c.Email).HasMaxLength(254);
        builder.Property(c => c.Phone).HasMaxLength(32);
        builder.Property(c => c.Nationality).HasMaxLength(2).IsFixedLength();
        builder.Property(c => c.LicenseCountry).HasMaxLength(2).IsFixedLength();
        builder.Property(c => c.LicenseNumber).HasMaxLength(32);
    }
}
