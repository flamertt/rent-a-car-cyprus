using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Auth.Domain.Entities;

namespace RentACar.Modules.Auth.Application.Abstractions;

internal interface IAuthDbContext
{
    DbSet<User> Users { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
