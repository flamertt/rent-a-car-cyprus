using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RentACar.Shared.Abstractions.Modules;
using RentACar.Shared.Infrastructure.Messaging;

namespace RentACar.Shared.Infrastructure.Modules;

public static class ModuleExtensions
{
    /// <summary>Modülleri ve işleyicilerini kaydeder; aynı örnekler <see cref="MapModules"/> ile uç noktalarını açar.</summary>
    public static IServiceCollection AddModules(this IServiceCollection services, IConfiguration configuration, IReadOnlyList<IModule> modules)
    {
        foreach (var module in modules)
        {
            module.Register(services, configuration);
            services.AddModuleHandlers(module.GetType().Assembly);
        }

        services.AddSingleton(modules);
        return services;
    }

    public static IEndpointRouteBuilder MapModules(this IEndpointRouteBuilder endpoints)
    {
        foreach (var module in endpoints.ServiceProvider.GetRequiredService<IReadOnlyList<IModule>>())
        {
            module.MapEndpoints(endpoints);
        }

        return endpoints;
    }

    /// <summary>Migration + tohum veri; modüller <see cref="IModuleInitializer.Order"/> sırasıyla.</summary>
    public static async Task InitializeModulesAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("ModuleInitializer");
        foreach (var initializer in scope.ServiceProvider.GetServices<IModuleInitializer>().OrderBy(i => i.Order))
        {
            logger.LogInformation("Modül hazırlanıyor: {Initializer}", initializer.GetType().Name);
            await initializer.InitializeAsync(cancellationToken);
        }
    }

    public static bool ShouldAutoMigrate(this IHostEnvironment environment, IConfiguration configuration) =>
        configuration.GetValue("Database:AutoMigrate", environment.IsDevelopment());
}
