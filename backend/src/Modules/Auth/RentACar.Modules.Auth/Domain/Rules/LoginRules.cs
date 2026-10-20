using RentACar.Shared.Abstractions.Exceptions;

namespace RentACar.Modules.Auth.Domain.Rules;

/// <summary>Giriş ve oturum kuralları.</summary>
internal static class LoginRules
{
    /// <summary>Bu kadar ardışık hatalı denemede hesap <see cref="LockoutDuration"/> kilitlenir.</summary>
    public const int MaxFailedAttempts = 5;

    public const int MinPasswordLength = 8;

    public const int MaxPasswordLength = 128;

    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Döndürülmüş yenileme belirteci bu süre içinde yeniden gelirse (iki sekme aynı anda yeniledi) aynı aileden
    /// yeni belirteç verilir; daha geç gelirse çalınmış sayılır ve ailenin tüm oturumları kapatılır.
    /// </summary>
    public static readonly TimeSpan RefreshReuseGrace = TimeSpan.FromSeconds(30);

    public static void EnsurePasswordIsAcceptable(string? password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
        {
            throw new DomainException("auth.weak_password", $"Şifre en az {MinPasswordLength} karakter olmalı.");
        }

        if (password.Length > MaxPasswordLength)
        {
            throw new DomainException("auth.password_too_long", $"Şifre en fazla {MaxPasswordLength} karakter olabilir.");
        }
    }
}
