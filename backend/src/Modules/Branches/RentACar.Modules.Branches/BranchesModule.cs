using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RentACar.Modules.Branches.Application.Abstractions;
using RentACar.Modules.Branches.Application.Services;
using RentACar.Modules.Branches.Contracts;
using RentACar.Modules.Branches.Endpoints;
using RentACar.Modules.Branches.Infrastructure.Persistence;
using RentACar.Shared.Abstractions.Modules;
using RentACar.Shared.Infrastructure.Database;

namespace RentACar.Modules.Branches;

/// <summary>Şubeler: ofisler, havalimanı/liman karşılama ve sınır kapısı teslim noktaları (PostGIS konumlu).</summary>
public sealed class BranchesModule : IModule
{
    public string Name => "branches";

    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<BranchesDbContext>(configuration, BranchesDbContext.SchemaName);
        services.AddScoped<IBranchesDbContext>(sp => sp.GetRequiredService<BranchesDbContext>());
        services.AddScoped<IBranchesApi, BranchesModuleApi>();
        services.AddScoped<IModuleInitializer, BranchesInitializer>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        PublicBranchEndpoints.Map(endpoints);
        AdminBranchEndpoints.Map(endpoints);
    }
}
