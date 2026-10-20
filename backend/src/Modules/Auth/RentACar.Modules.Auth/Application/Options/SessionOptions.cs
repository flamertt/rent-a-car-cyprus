namespace RentACar.Modules.Auth.Application.Options;

/// <summary>appsettings: <c>Auth</c> bölümünün oturum ayarları (JWT ayarları Shared'daki AuthOptions'ta).</summary>
internal sealed class SessionOptions
{
    public const string SectionName = "Auth";

    /// <summary>Yenileme belirteci ömrü; her yenilemede baştan başlar (kayan oturum).</summary>
    public int RefreshTokenDays { get; set; } = 7;
}
