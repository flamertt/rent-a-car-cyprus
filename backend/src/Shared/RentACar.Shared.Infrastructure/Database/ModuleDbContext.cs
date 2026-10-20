using Microsoft.EntityFrameworkCore;

namespace RentACar.Shared.Infrastructure.Database;

/// <summary>
/// Modül DbContext'lerinin tabanı: her modül kendi PostgreSQL şemasında yaşar
/// (branches, fleet, pricing, …) ve başka modülün tablolarına yabancı anahtar kurmaz;
/// modüller arası bağ yalnız kimlik (Guid) ve Contracts arayüzleriyle.
/// </summary>
public abstract class ModuleDbContext(DbContextOptions options) : DbContext(options)
{
    protected abstract string Schema { get; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Enum'lar okunabilir metin olarak saklanır (admin paneli / psql'de anlamlı).
        configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(32);
        configurationBuilder.Properties<decimal>().HavePrecision(12, 2);
        configurationBuilder.Properties<string>().HaveMaxLength(256);
    }
}
