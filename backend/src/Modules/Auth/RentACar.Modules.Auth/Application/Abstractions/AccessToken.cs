namespace RentACar.Modules.Auth.Application.Abstractions;

internal sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);
