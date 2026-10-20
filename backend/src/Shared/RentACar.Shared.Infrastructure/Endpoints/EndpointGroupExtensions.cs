using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RentACar.Shared.Infrastructure.Auth;

namespace RentACar.Shared.Infrastructure.Endpoints;

public static class EndpointGroupExtensions
{
    /// <summary>Herkese açık uç noktalar: <c>/api/{module}</c> (giriş gerekmez).</summary>
    public static RouteGroupBuilder MapPublicGroup(this IEndpointRouteBuilder endpoints, string module, string tag) =>
        endpoints.MapGroup($"/api/{module}").WithTags(tag).AllowAnonymous();

    /// <summary>
    /// Yönetim paneli uç noktaları: <c>/api/admin/{module}</c>. Yetki politikası yalnız burada bağlanır
    /// (tüm modüller tek yerden korunur): geçerli bir erişim belirteci (JWT Bearer) gerekir.
    /// </summary>
    public static RouteGroupBuilder MapAdminGroup(this IEndpointRouteBuilder endpoints, string module, string tag) =>
        endpoints.MapGroup($"/api/admin/{module}").WithTags($"Admin · {tag}").RequireAuthorization(AuthPolicies.AdminPanel);
}
