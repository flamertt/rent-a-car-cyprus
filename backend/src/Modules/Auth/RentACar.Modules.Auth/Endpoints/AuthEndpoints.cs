using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using RentACar.Modules.Auth.Application.Commands.ChangePassword;
using RentACar.Modules.Auth.Application.Commands.Login;
using RentACar.Modules.Auth.Application.Commands.Logout;
using RentACar.Modules.Auth.Application.Commands.RefreshSession;
using RentACar.Modules.Auth.Application.Dtos;
using RentACar.Modules.Auth.Application.Queries.GetCurrentUser;
using RentACar.Modules.Auth.Endpoints.Requests;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Infrastructure.Auth;

namespace RentACar.Modules.Auth.Endpoints;

/// <summary>
/// <c>/api/auth</c>: erişim belirteci (JWT, kısa ömürlü) yanıt gövdesinde döner ve <c>Authorization: Bearer</c>
/// ile gönderilir; yenileme belirteci yalnız HttpOnly çerezde taşınır.
/// </summary>
internal static class AuthEndpoints
{
    public const string LoginRateLimitPolicy = "auth-login";

    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth").WithTags("Kimlik doğrulama");

        group.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .RequireRateLimiting(LoginRateLimitPolicy)
            .WithSummary("E-posta + şifre ile giriş: erişim belirteci (gövde) + yenileme belirteci (HttpOnly çerez)");
        group.MapPost("/refresh", RefreshAsync)
            .AllowAnonymous()
            .WithSummary("Çerezdeki yenileme belirteciyle yeni erişim belirteci (yenileme belirteci döndürülür)");
        group.MapPost("/logout", LogoutAsync)
            .AllowAnonymous()
            .WithSummary("Çıkış: bu oturumu kapatır, çerezi siler");
        group.MapGet("/me", MeAsync)
            .RequireAuthorization(AuthPolicies.AdminPanel)
            .WithSummary("Giriş yapmış kullanıcı");
        group.MapPost("/change-password", ChangePasswordAsync)
            .RequireAuthorization(AuthPolicies.AdminPanel)
            .WithSummary("Şifre değiştir: kullanıcının tüm oturumları kapanır, yeniden giriş gerekir");
    }

    private static async Task<Ok<AuthSessionDto>> LoginAsync(LoginRequest r, HttpContext http, IDispatcher dispatcher, CancellationToken ct)
    {
        var result = await dispatcher.SendAsync(new LoginCommand(r.Email, r.Password), ct);
        RefreshTokenCookie.Append(http, result.RefreshToken, result.RefreshTokenExpiresAt);
        return TypedResults.Ok(result.Session);
    }

    private static async Task<Results<Ok<AuthSessionDto>, ProblemHttpResult>> RefreshAsync(HttpContext http, IDispatcher dispatcher, CancellationToken ct)
    {
        try
        {
            var result = await dispatcher.SendAsync(new RefreshSessionCommand(RefreshTokenCookie.Read(http)), ct);
            RefreshTokenCookie.Append(http, result.RefreshToken, result.RefreshTokenExpiresAt);
            return TypedResults.Ok(result.Session);
        }
        catch (UnauthorizedException e)
        {
            // İstisna işleyicisi yanıt başlıklarını temizler; geçersiz çerezi silebilmek için 401 burada yazılır.
            RefreshTokenCookie.Delete(http);
            return TypedResults.Problem(
                statusCode: StatusCodes.Status401Unauthorized, title: e.Message, extensions: new Dictionary<string, object?> { ["code"] = e.Code });
        }
    }

    private static async Task<NoContent> LogoutAsync(HttpContext http, IDispatcher dispatcher, CancellationToken ct)
    {
        await dispatcher.SendAsync(new LogoutCommand(RefreshTokenCookie.Read(http)), ct);
        RefreshTokenCookie.Delete(http);
        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<UserDto>, UnauthorizedHttpResult>> MeAsync(ClaimsPrincipal principal, IDispatcher dispatcher, CancellationToken ct) =>
        principal.GetUserId() is { } id && await dispatcher.QueryAsync(new GetCurrentUserQuery(id), ct) is { } user
            ? TypedResults.Ok(user)
            : TypedResults.Unauthorized();

    private static async Task<Results<NoContent, UnauthorizedHttpResult>> ChangePasswordAsync(
        ChangePasswordRequest r, ClaimsPrincipal principal, HttpContext http, IDispatcher dispatcher, CancellationToken ct)
    {
        if (principal.GetUserId() is not { } id)
        {
            return TypedResults.Unauthorized();
        }

        await dispatcher.SendAsync(new ChangePasswordCommand(id, r.CurrentPassword, r.NewPassword), ct);
        RefreshTokenCookie.Delete(http);
        return TypedResults.NoContent();
    }
}
