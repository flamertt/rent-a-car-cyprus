using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RentACar.Modules.Customers.Application.Abstractions;
using RentACar.Modules.Customers.Application.Services;
using RentACar.Modules.Customers.Contracts;
using RentACar.Modules.Customers.Endpoints;
using RentACar.Modules.Customers.Infrastructure.Persistence;
using RentACar.Shared.Abstractions.Modules;
using RentACar.Shared.Infrastructure.Database;

namespace RentACar.Modules.Customers;

/// <summary>Müşteriler: kimlik, ehliyet, yaş/ehliyet kuralları (simülasyonun sahte müşterileri dahil).</summary>
public sealed class CustomersModule : IModule
{
    public string Name => "customers";

    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<CustomersDbContext>(configuration, CustomersDbContext.SchemaName);
        services.AddScoped<ICustomersDbContext>(sp => sp.GetRequiredService<CustomersDbContext>());
        services.AddScoped<ICustomersApi, CustomersModuleApi>();
        services.AddScoped<IModuleInitializer, CustomersInitializer>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) => AdminCustomerEndpoints.Map(endpoints);
}
