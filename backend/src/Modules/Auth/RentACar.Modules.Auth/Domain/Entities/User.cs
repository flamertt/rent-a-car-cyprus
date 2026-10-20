using RentACar.Modules.Auth.Domain.Rules;
using RentACar.Shared.Abstractions.Exceptions;

namespace RentACar.Modules.Auth.Domain.Entities;

/// <summary>Yönetim paneli kullanıcısı. Şifre yalnız özet olarak tutulur; özeti uygulama katmanı üretir.</summary>
internal sealed class User
{
    private User()
    {
    }

    public Guid Id { get; private set; }

    /// <summary>Küçük harfe normalize; benzersiz. Giriş anahtarı.</summary>
    public string Email { get; private set; } = null!;

    public string DisplayName { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public bool IsActive { get; private set; }

    /// <summary>Son başarılı girişten (ya da son kilitten) bu yana ardışık hatalı deneme.</summary>
    public int FailedLoginCount { get; private set; }

    public DateTimeOffset? LockedUntil { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? LastLoginAt { get; private set; }

    public DateTimeOffset PasswordChangedAt { get; private set; }

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public static User Create(string email, string displayName, string passwordHash, DateTimeOffset now)
    {
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = NormalizeEmail(email),
            PasswordHash = passwordHash,
            IsActive = true,
            CreatedAt = now,
            PasswordChangedAt = now,
        };
        if (!user.Email.Contains('@') || user.Email.StartsWith('@') || user.Email.EndsWith('@'))
        {
            throw new DomainException("auth.invalid_email", "Geçerli bir e-posta adresi girin.");
        }

        user.Rename(displayName);
        return user;
    }

    public void Rename(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new DomainException("auth.name_required", "Görünen ad zorunlu.");
        }

        DisplayName = displayName.Trim();
    }

    public bool IsLockedOut(DateTimeOffset now) => LockedUntil > now;

    public void RecordFailedLogin(DateTimeOffset now)
    {
        FailedLoginCount++;
        if (FailedLoginCount >= LoginRules.MaxFailedAttempts)
        {
            LockedUntil = now + LoginRules.LockoutDuration;
            FailedLoginCount = 0;
        }
    }

    public void RecordLogin(DateTimeOffset now)
    {
        FailedLoginCount = 0;
        LockedUntil = null;
        LastLoginAt = now;
    }

    public void ChangePassword(string passwordHash, DateTimeOffset now)
    {
        PasswordHash = passwordHash;
        PasswordChangedAt = now;
    }

    /// <summary>Aynı şifrenin güncel algoritma/iterasyonla yeniden özetlenmesi (şifre değişmiş sayılmaz).</summary>
    public void UpgradePasswordHash(string passwordHash) => PasswordHash = passwordHash;
}
