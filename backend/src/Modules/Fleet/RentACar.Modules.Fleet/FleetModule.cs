using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RentACar.Modules.Fleet.Application.Abstractions;
using RentACar.Modules.Fleet.Application.Services;
using RentACar.Modules.Fleet.Contracts;
using RentACar.Modules.Fleet.Endpoints;
using RentACar.Modules.Fleet.Infrastructure.Live;
using RentACar.Modules.Fleet.Infrastructure.Options;
using RentACar.Modules.Fleet.Infrastructure.Persistence;
using RentACar.Shared.Abstractions.Modules;
using RentACar.Shared.Infrastructure.Database;

namespace RentACar.Modules.Fleet;

/// <summary>Filo: ACRISS sınıfları, araçlar, fiziksel durum, telemetri ve canlı konum akışı.</summary>
public sealed class FleetModule : IModule
{
    public string Name => "fleet";

    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<FleetDbContext>(configuration, FleetDbContext.SchemaName);
        services.AddScoped<IFleetDbContext>(sp => sp.GetRequiredService<FleetDbContext>());
        services.Configure<FleetSeedOptions>(configuration.GetSection(FleetSeedOptions.SectionName));
        services.AddSingleton<IFleetLiveFeed, FleetLiveFeed>();
        services.AddScoped<IFleetApi, FleetModuleApi>();
        services.AddScoped<ParkingAllocator>();
        services.AddScoped<IModuleInitializer, FleetInitializer>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        PublicFleetEndpoints.Map(endpoints);
        AdminFleetEndpoints.Map(endpoints);
    }
}
