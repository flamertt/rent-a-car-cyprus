using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Auth.Application.Abstractions;
using RentACar.Modules.Auth.Application.Mappings;
using RentACar.Modules.Auth.Application.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Auth.Application.Queries.GetCurrentUser;

internal sealed class GetCurrentUserHandler(IAuthDbContext db) : IQueryHandler<GetCurrentUserQuery, UserDto?>
{
    public async Task<UserDto?> HandleAsync(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == query.UserId && u.IsActive, cancellationToken);
        return user?.ToDto();
    }
}
