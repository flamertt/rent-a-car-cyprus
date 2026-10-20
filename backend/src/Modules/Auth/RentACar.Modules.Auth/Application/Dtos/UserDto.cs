namespace RentACar.Modules.Auth.Application.Dtos;

internal sealed record UserDto(Guid Id, string Email, string DisplayName, DateTimeOffset? LastLoginAt);
