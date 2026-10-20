using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RentACar.Modules.Reservations.Application.Abstractions;
using RentACar.Modules.Reservations.Application.Services;
using RentACar.Modules.Reservations.Contracts;
using RentACar.Modules.Reservations.Domain.Rules;
using RentACar.Modules.Reservations.Endpoints;
using RentACar.Modules.Reservations.Infrastructure.BackgroundJobs;
using RentACar.Modules.Reservations.Infrastructure.Persistence;
using RentACar.Shared.Abstractions.Modules;
using RentACar.Shared.Infrastructure.Database;

namespace RentACar.Modules.Reservations;

/// <summary>Rezervasyon ve kiralama: araç atama, teslim/iade, geç iade + eksik yakıt, gelmedi taraması, pano.</summary>
public sealed class ReservationsModule : IModule
{
    public string Name => "reservations";

    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<ReservationsDbContext>(configuration, ReservationsDbContext.SchemaName);
        services.AddScoped<IReservationsDbContext>(sp => sp.GetRequiredService<ReservationsDbContext>());
        services.Configure<ReservationRules>(configuration.GetSection(ReservationRules.SectionName));
        services.AddScoped<VehicleAvailability>();
        services.AddScoped<ActiveBranchGuard>();
        services.AddScoped<CustomerResolver>();
        services.AddScoped<IReservationsApi, ReservationsModuleApi>();
        services.AddScoped<IModuleInitializer, ReservationsInitializer>();
        services.AddHostedService<NoShowSweeper>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) => AdminReservationEndpoints.Map(endpoints);
}
