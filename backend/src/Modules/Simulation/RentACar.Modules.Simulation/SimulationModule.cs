using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RentACar.Modules.Simulation.Application.Abstractions;
using RentACar.Modules.Simulation.Application.Engine;
using RentACar.Modules.Simulation.Application.Options;
using RentACar.Modules.Simulation.Application.Services;
using RentACar.Modules.Simulation.Endpoints;
using RentACar.Modules.Simulation.Infrastructure.Hosting;
using RentACar.Modules.Simulation.Infrastructure.Persistence;
using RentACar.Modules.Simulation.Infrastructure.Places;
using RentACar.Modules.Simulation.Infrastructure.Routing;
using RentACar.Shared.Abstractions.Modules;
using RentACar.Shared.Infrastructure.Database;

namespace RentACar.Modules.Simulation;

/// <summary>
/// Şirketin "yaşayan" operasyonu: rastgele müşteriler rezervasyon yapar, araçlar teslim noktalarına götürülür,
/// müşteriler KKTC'yi gezer, iade eder. Diğer modüllere yalnız Contracts üzerinden erişir; Contracts projesi
/// yoktur (bu modülü kimse çağırmaz).
/// </summary>
public sealed class SimulationModule : IModule
{
    public string Name => "simulation";

    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<SimulationDbContext>(configuration, SimulationDbContext.SchemaName);
        services.AddScoped<ISimulationDbContext>(sp => sp.GetRequiredService<SimulationDbContext>());
        services.Configure<SimulationOptions>(configuration.GetSection(SimulationOptions.SectionName));
        services.AddHttpClient<IRouteProvider, OsrmRouteProvider>();
        services.AddSingleton<IPlacesCatalog, EmbeddedPlacesCatalog>();
        services.AddSingleton<RouteMemory>();
        services.AddSingleton<SimulationRuntime>();
        services.AddScoped<RouteService>();
        services.AddScoped<EventLog>();
        services.AddScoped<DemandGenerator>();
        services.AddScoped<FleetDispatcher>();
        services.AddScoped<ActivityFloor>();
        services.AddScoped<TripRunner>();
        services.AddScoped<SimulationTick>();
        services.AddScoped<IModuleInitializer, SimulationInitializer>();
        services.AddHostedService<SimulationHostedService>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ClockEndpoints.Map(endpoints);
        AdminSimulationEndpoints.Map(endpoints);
    }
}
