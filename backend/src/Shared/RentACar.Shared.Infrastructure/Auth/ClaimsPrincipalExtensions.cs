using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace RentACar.Shared.Infrastructure.Auth;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Erişim belirtecindeki <c>sub</c> (kullanıcı kimliği); yoksa ya da Guid değilse null.</summary>
    public static Guid? GetUserId(this ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : null;
}
