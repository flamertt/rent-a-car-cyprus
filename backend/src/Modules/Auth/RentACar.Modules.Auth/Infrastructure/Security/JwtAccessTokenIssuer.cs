using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RentACar.Modules.Auth.Application.Abstractions;
using RentACar.Modules.Auth.Domain.Entities;
using RentACar.Shared.Infrastructure.Auth;

namespace RentACar.Modules.Auth.Infrastructure.Security;

/// <summary>HS256 imzalı JWT; doğrulaması Shared'daki JWT Bearer ayarlarıyla (aynı <see cref="AuthOptions"/>).</summary>
internal sealed class JwtAccessTokenIssuer(IOptions<AuthOptions> options) : IAccessTokenIssuer
{
    private readonly JsonWebTokenHandler _handler = new();

    public AccessToken Issue(User user, DateTimeOffset now)
    {
        var o = options.Value;
        var expiresAt = now.AddMinutes(o.AccessTokenMinutes);
        var token = _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = o.Issuer,
            Audience = o.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
                [JwtRegisteredClaimNames.Email] = user.Email,
                [JwtRegisteredClaimNames.Name] = user.DisplayName,
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString("N"),
            },
            SigningCredentials = new SigningCredentials(o.CreateSigningKey(), SecurityAlgorithms.HmacSha256),
        });
        return new AccessToken(token, expiresAt);
    }
}
