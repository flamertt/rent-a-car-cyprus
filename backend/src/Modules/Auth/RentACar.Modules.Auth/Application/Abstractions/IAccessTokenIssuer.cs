using RentACar.Modules.Auth.Domain.Entities;

namespace RentACar.Modules.Auth.Application.Abstractions;

/// <summary>Kısa ömürlü erişim belirteci (JWT) üretir.</summary>
internal interface IAccessTokenIssuer
{
    AccessToken Issue(User user, DateTimeOffset now);
}
