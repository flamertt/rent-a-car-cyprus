namespace RentACar.Modules.Auth.Application.Dtos;

/// <summary>Oturum açan/yenileyen komutların sonucu; uç nokta yenileme belirtecini çereze yazar.</summary>
internal sealed record AuthResult(AuthSessionDto Session, string RefreshToken, DateTimeOffset RefreshTokenExpiresAt);
