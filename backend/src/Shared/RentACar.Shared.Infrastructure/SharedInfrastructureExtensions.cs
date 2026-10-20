using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RentACar.Shared.Abstractions.Events;
using RentACar.Shared.Abstractions.Modules;
using RentACar.Shared.Abstractions.Time;
using RentACar.Shared.Infrastructure.Errors;
using RentACar.Shared.Infrastructure.Events;
using RentACar.Shared.Infrastructure.Time;

namespace RentACar.Shared.Infrastructure;

public static class SharedInfrastructureExtensions
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ClockOptions>(configuration.GetSection(ClockOptions.SectionName));
        services.AddSingleton<IClock, SimulationClock>();
        services.AddScoped<IEventBus, InProcessEventBus>();
        services.AddProblemDetails();
        services.AddExceptionHandler<AppExceptionHandler>();
        services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        return services;
    }

    /// <summary>Modülleri kaydeder; aynı örnekler <see cref="MapModules"/> ile uç noktalarını açar.</summary>
    public static IServiceCollection AddModules(this IServiceCollection services, IConfiguration configuration, IReadOnlyList<IModule> modules)
    {
        foreach (var module in modules)
        {
            module.Register(services, configuration);
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

    /// <summary>Herkese açık uç noktalar: <c>/api/{module}</c>.</summary>
    public static RouteGroupBuilder MapPublicGroup(this IEndpointRouteBuilder endpoints, string module, string tag) =>
        endpoints.MapGroup($"/api/{module}").WithTags(tag);

    /// <summary>
    /// Yönetim paneli uç noktaları: <c>/api/admin/{module}</c>. Kimlik doğrulama eklendiğinde
    /// yetki politikası yalnız burada bağlanır (tüm modüller tek yerden korunur).
    /// </summary>
    public static RouteGroupBuilder MapAdminGroup(this IEndpointRouteBuilder endpoints, string module, string tag) =>
        endpoints.MapGroup($"/api/admin/{module}").WithTags($"Admin · {tag}");

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
