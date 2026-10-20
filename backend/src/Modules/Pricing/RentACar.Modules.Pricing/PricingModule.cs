using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RentACar.Modules.Pricing.Application.Abstractions;
using RentACar.Modules.Pricing.Application.Services;
using RentACar.Modules.Pricing.Contracts;
using RentACar.Modules.Pricing.Endpoints;
using RentACar.Modules.Pricing.Infrastructure.Persistence;
using RentACar.Shared.Abstractions.Modules;
using RentACar.Shared.Infrastructure.Database;

namespace RentACar.Modules.Pricing;

/// <summary>Fiyatlandırma: sezon, süre indirimi, ekstralar, teslim/tek yön/mesai dışı ücretleri, genç sürücü.</summary>
public sealed class PricingModule : IModule
{
    public string Name => "pricing";

    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<PricingDbContext>(configuration, PricingDbContext.SchemaName);
        services.AddScoped<IPricingDbContext>(sp => sp.GetRequiredService<PricingDbContext>());
        services.AddScoped<IPricingApi, PricingModuleApi>();
        services.AddScoped<IModuleInitializer, PricingInitializer>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        PublicPricingEndpoints.Map(endpoints);
        AdminPricingEndpoints.Map(endpoints);
    }
}
