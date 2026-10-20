namespace RentACar.Shared.Abstractions.Exceptions;

/// <summary>Kimlik doğrulanamadı (hatalı giriş, geçersiz/süresi dolmuş oturum) → HTTP 401.</summary>
public sealed class UnauthorizedException(string code, string message) : DomainException(code, message);
