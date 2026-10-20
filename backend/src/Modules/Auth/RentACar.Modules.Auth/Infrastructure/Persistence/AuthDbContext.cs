using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Auth.Application.Abstractions;
using RentACar.Modules.Auth.Domain.Entities;
using RentACar.Shared.Infrastructure.Database;

namespace RentACar.Modules.Auth.Infrastructure.Persistence;

internal sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : ModuleDbContext(options), IAuthDbContext
{
    public const string SchemaName = "auth";

    protected override string Schema => SchemaName;

    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
}
