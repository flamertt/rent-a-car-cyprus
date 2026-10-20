using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RentACar.Shared.Infrastructure.Database;

public static class DatabaseExtensions
{
    public const string ConnectionStringName = "Postgres";

    /// <summary>
    /// Modül DbContext'ini kaydeder: ortak bağlantı, PostGIS (NetTopologySuite), snake_case adlar
    /// ve modülün kendi şemasında migration geçmişi (modüller bağımsız migrate edilir).
    /// </summary>
    public static IServiceCollection AddModuleDbContext<TContext>(
        this IServiceCollection services, IConfiguration configuration, string schema)
        where TContext : ModuleDbContext
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"ConnectionStrings:{ConnectionStringName} tanımlı değil.");

        services.AddDbContext<TContext>(options => options
            .UseNpgsql(connectionString, npgsql => npgsql
                .UseNetTopologySuite()
                .MigrationsHistoryTable("__ef_migrations_history", schema))
            .UseSnakeCaseNamingConvention());

        return services;
    }
}
