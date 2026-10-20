using Microsoft.AspNetCore.Identity;
using RentACar.Modules.Auth.Application.Abstractions;
using RentACar.Modules.Auth.Domain.Entities;

namespace RentACar.Modules.Auth.Infrastructure.Security;

/// <summary>
/// ASP.NET Core Identity özet biçimi (PBKDF2-HMAC-SHA512, tuzlu, sürüm baytlı). İterasyon sayısı artarsa
/// eski özetler girişte <c>needsRehash</c> ile güncellenir.
/// </summary>
internal sealed class IdentityPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _inner = new();
    private readonly Lazy<string> _dummyHash;

    public IdentityPasswordHasher() => _dummyHash = new Lazy<string>(() => Hash(Guid.NewGuid().ToString()));

    public string Hash(string password) => _inner.HashPassword(null!, password);

    public bool Verify(string? passwordHash, string password, out bool needsRehash)
    {
        if (passwordHash is null)
        {
            _inner.VerifyHashedPassword(null!, _dummyHash.Value, password);
            needsRehash = false;
            return false;
        }

        var result = _inner.VerifyHashedPassword(null!, passwordHash, password);
        needsRehash = result == PasswordVerificationResult.SuccessRehashNeeded;
        return result != PasswordVerificationResult.Failed;
    }
}
