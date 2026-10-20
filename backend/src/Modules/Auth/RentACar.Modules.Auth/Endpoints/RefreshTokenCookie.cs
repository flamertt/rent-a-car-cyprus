using Microsoft.AspNetCore.Http;

namespace RentACar.Modules.Auth.Endpoints;

/// <summary>
/// Yenileme belirteci çerezi: HttpOnly (betik okuyamaz), SameSite=Strict (başka siteden gönderilmez) ve yalnız
/// <c>/api/auth</c> yoluna gider; diğer API çağrılarında taşınmaz.
/// </summary>
internal static class RefreshTokenCookie
{
    public const string Name = "rac_refresh";

    private const string CookiePath = "/api/auth";

    public static string? Read(HttpContext http) => http.Request.Cookies[Name];

    public static void Append(HttpContext http, string token, DateTimeOffset expiresAt) =>
        http.Response.Cookies.Append(Name, token, Options(http, expiresAt));

    public static void Delete(HttpContext http) => http.Response.Cookies.Delete(Name, Options(http, expiresAt: null));

    private static CookieOptions Options(HttpContext http, DateTimeOffset? expiresAt) => new()
    {
        HttpOnly = true,
        Secure = http.Request.IsHttps,
        SameSite = SameSiteMode.Strict,
        Path = CookiePath,
        Expires = expiresAt,
        IsEssential = true,
    };
}
