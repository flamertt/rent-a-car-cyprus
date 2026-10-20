using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RentACar.Modules.Auth.Application.Abstractions;
using RentACar.Modules.Auth.Application.Options;
using RentACar.Modules.Auth.Application.Services;
using RentACar.Modules.Auth.Endpoints;
using RentACar.Modules.Auth.Infrastructure.Options;
using RentACar.Modules.Auth.Infrastructure.Persistence;
using RentACar.Modules.Auth.Infrastructure.Security;
using RentACar.Shared.Abstractions.Modules;
using RentACar.Shared.Infrastructure.Database;

namespace RentACar.Modules.Auth;

/// <summary>
/// Panel kullanıcıları ve oturum: giriş, döndürmeli yenileme belirteci, çıkış, şifre değişimi. Erişim belirtecini
/// (JWT) üretir; doğrulaması Shared altyapısında. Contracts projesi yoktur (bu modülü başka modül çağırmaz).
/// </summary>
public sealed class AuthModule : IModule
{
    public string Name => "auth";

    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<AuthDbContext>(configuration, AuthDbContext.SchemaName);
        services.AddScoped<IAuthDbContext>(sp => sp.GetRequiredService<AuthDbContext>());
        services.Configure<SessionOptions>(configuration.GetSection(SessionOptions.SectionName));
        services.Configure<SeedAdminOptions>(configuration.GetSection(SeedAdminOptions.SectionName));
        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();
        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();
        services.AddScoped<SessionIssuer>();
        services.AddScoped<IModuleInitializer, AuthInitializer>();

        // Kaba kuvvete karşı IP başına dakikada 10 giriş denemesi (hesap kilidi ayrıca LoginRules'ta).
        services.Configure<RateLimiterOptions>(o => o.AddPolicy(AuthEndpoints.LoginRateLimitPolicy, http =>
            RateLimitPartition.GetFixedWindowLimiter(
                http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) })));
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) => AuthEndpoints.Map(endpoints);
}
