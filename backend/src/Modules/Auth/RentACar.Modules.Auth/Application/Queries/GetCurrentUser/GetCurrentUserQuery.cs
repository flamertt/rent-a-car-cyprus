using RentACar.Modules.Auth.Application.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Auth.Application.Queries.GetCurrentUser;

internal sealed record GetCurrentUserQuery(Guid UserId) : IQuery<UserDto?>;
