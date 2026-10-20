using RentACar.Modules.Auth.Application.Dtos;
using RentACar.Modules.Auth.Domain.Entities;

namespace RentACar.Modules.Auth.Application.Mappings;

internal static class UserMappings
{
    public static UserDto ToDto(this User u) => new(u.Id, u.Email, u.DisplayName, u.LastLoginAt);
}
