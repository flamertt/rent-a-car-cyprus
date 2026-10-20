using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace RentACar.Shared.Infrastructure.Auth;

public static class AuthenticationExtensions
{
    /// <summary>
    /// JWT Bearer doğrulaması + yetki politikaları. Belirteci Auth modülü üretir; burada yalnız doğrulanır.
    /// Güvenli varsayılan: grubu/özniteliği olmayan uç nokta da giriş ister (fallback politikası), herkese
    /// açık olanlar <c>AllowAnonymous</c> ile işaretlenir.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AuthOptions>()
            .Bind(configuration.GetSection(AuthOptions.SectionName))
            .Validate(o => Encoding.UTF8.GetByteCount(o.SigningKey) >= AuthOptions.MinSigningKeyBytes,
                $"Auth:SigningKey en az {AuthOptions.MinSigningKeyBytes} bayt olmalı (ortam değişkeni: Auth__SigningKey).")
            .Validate(o => o.AccessTokenMinutes > 0, "Auth:AccessTokenMinutes pozitif olmalı.")
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<AuthOptions>>((jwt, auth) =>
            {
                var o = auth.Value;
                // Talep adları belirteçteki gibi kalsın (sub, email, name); ClaimTypes.* eşlemesi yok.
                jwt.MapInboundClaims = false;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = o.Issuer,
                    ValidAudience = o.Audience,
                    IssuerSigningKey = o.CreateSigningKey(),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    NameClaimType = JwtRegisteredClaimNames.Name,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthPolicies.AdminPanel, p => p.RequireAuthenticatedUser())
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        return services;
    }
}
