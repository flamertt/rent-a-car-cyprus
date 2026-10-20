namespace RentACar.Modules.Auth.Application.Dtos;

/// <summary>Giriş/yenileme yanıtı. Yenileme belirteci gövdede yok: HttpOnly çerezde gider.</summary>
internal sealed record AuthSessionDto(string AccessToken, DateTimeOffset ExpiresAt, UserDto User);
