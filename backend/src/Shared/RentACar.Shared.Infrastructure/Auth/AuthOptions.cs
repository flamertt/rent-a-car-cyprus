using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace RentACar.Shared.Infrastructure.Auth;

/// <summary>appsettings: <c>Auth</c> bölümü. Erişim belirteci (JWT) hem burada doğrulanır hem Auth modülünde üretilir.</summary>
public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>HMAC-SHA256 için en az 32 bayt.</summary>
    public const int MinSigningKeyBytes = 32;

    public string Issuer { get; set; } = "mert-peker-rent-a-car";

    public string Audience { get; set; } = "mert-peker-rent-a-car-panel";

    /// <summary>Gizli imza anahtarı. Geliştirmede appsettings.Development.json; başka ortamda <c>Auth__SigningKey</c> ortam değişkeni.</summary>
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;

    public SymmetricSecurityKey CreateSigningKey() => new(Encoding.UTF8.GetBytes(SigningKey));
}
