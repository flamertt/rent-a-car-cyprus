using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentACar.Modules.Auth.Application.Abstractions;
using RentACar.Modules.Auth.Domain.Entities;
using RentACar.Modules.Auth.Domain.Rules;
using RentACar.Modules.Auth.Infrastructure.Options;
using RentACar.Shared.Abstractions.Modules;

namespace RentACar.Modules.Auth.Infrastructure.Persistence;

/// <summary>Migration + varsayılan yönetici (yalnız hiç kullanıcı yokken; şifresi sonradan panelden değiştirilir).</summary>
internal sealed class AuthInitializer(
    AuthDbContext db, IPasswordHasher hasher, IOptions<SeedAdminOptions> seed, TimeProvider time, ILogger<AuthInitializer> logger)
    : IModuleInitializer
{
    public int Order => 5;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await db.Database.MigrateAsync(cancellationToken);
        if (await db.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var admin = seed.Value;
        if (string.IsNullOrWhiteSpace(admin.Email) || string.IsNullOrWhiteSpace(admin.Password))
        {
            logger.LogWarning("Hiç kullanıcı yok ve Auth:SeedAdmin tanımlı değil: panele giriş yapılamaz");
            return;
        }

        LoginRules.EnsurePasswordIsAcceptable(admin.Password);
        db.Users.Add(User.Create(admin.Email, admin.DisplayName, hasher.Hash(admin.Password), time.GetUtcNow()));
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Varsayılan yönetici oluşturuldu: {Email}", User.NormalizeEmail(admin.Email));
    }
}
