using Microsoft.Extensions.Options;
using RentACar.Modules.Auth.Application.Abstractions;
using RentACar.Modules.Auth.Application.Dtos;
using RentACar.Modules.Auth.Application.Mappings;
using RentACar.Modules.Auth.Application.Options;
using RentACar.Modules.Auth.Domain.Entities;

namespace RentACar.Modules.Auth.Application.Services;

/// <summary>Giriş ve yenilemenin ortak adımı: erişim belirteci + aileye yeni yenileme belirteci (kaydı çağıran yapar).</summary>
internal sealed class SessionIssuer(IAuthDbContext db, IAccessTokenIssuer accessTokens, IOptions<SessionOptions> options)
{
    public AuthResult Issue(User user, Guid familyId, DateTimeOffset now, RefreshToken? replacing = null)
    {
        var raw = RefreshTokenCodec.Generate();
        var refresh = RefreshToken.Issue(user.Id, familyId, RefreshTokenCodec.Hash(raw), now, TimeSpan.FromDays(options.Value.RefreshTokenDays));
        db.RefreshTokens.Add(refresh);
        replacing?.RotateTo(refresh.Id, now);

        var access = accessTokens.Issue(user, now);
        return new AuthResult(new AuthSessionDto(access.Token, access.ExpiresAt, user.ToDto()), raw, refresh.ExpiresAt);
    }
}
