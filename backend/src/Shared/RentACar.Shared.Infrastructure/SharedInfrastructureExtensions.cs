using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RentACar.Shared.Abstractions.Events;
using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Abstractions.Time;
using RentACar.Shared.Infrastructure.Auth;
using RentACar.Shared.Infrastructure.Errors;
using RentACar.Shared.Infrastructure.Events;
using RentACar.Shared.Infrastructure.Messaging;
using RentACar.Shared.Infrastructure.Time;

namespace RentACar.Shared.Infrastructure;

public static class SharedInfrastructureExtensions
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ClockOptions>(configuration.GetSection(ClockOptions.SectionName));
        services.AddSingleton<IClock, SimulationClock>();
        // Gerçek saat: belirteç süresi, hesap kilidi gibi güvenlik zamanları simülasyon saatine (IClock) bağlanmaz.
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IDispatcher, Dispatcher>();
        services.AddScoped<IEventBus, InProcessEventBus>();
        services.AddProblemDetails();
        services.AddExceptionHandler<AppExceptionHandler>();
        services.Configure<JsonOptions>(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddJwtAuthentication(configuration);
        // Modüller kendi politikalarını ekler (örn. giriş denemesi sınırı).
        services.AddRateLimiter(o => o.RejectionStatusCode = StatusCodes.Status429TooManyRequests);
        // Panel varsayılan olarak aynı origin'den (Vite proxy / ters vekil) gelir; ayrı origin gerekirse Cors:AllowedOrigins.
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(o =>
        {
            if (origins.Length > 0)
            {
                o.AddDefaultPolicy(p => p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
            }
        });
        return services;
    }
}
