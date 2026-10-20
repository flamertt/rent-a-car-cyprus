using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentACar.Modules.Auth.Application.Abstractions;
using RentACar.Modules.Auth.Application.Dtos;
using RentACar.Modules.Auth.Application.Services;
using RentACar.Modules.Auth.Domain.Entities;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Auth.Application.Commands.Login;

/// <summary>
/// E-posta + şifre ile giriş. Hatalı denemeler sayılır, <see cref="Domain.Rules.LoginRules.MaxFailedAttempts"/>
/// denemede hesap geçici kilitlenir. Kayıtsız e-posta ile yanlış şifre aynı yanıtı (ve benzer süreyi) alır.
/// </summary>
internal sealed class LoginHandler(
    IAuthDbContext db, IPasswordHasher hasher, SessionIssuer sessions, TimeProvider time, ILogger<LoginHandler> logger)
    : ICommandHandler<LoginCommand, AuthResult>
{
    public async Task<AuthResult> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        var now = time.GetUtcNow();
        var email = User.NormalizeEmail(command.Email ?? string.Empty);
        var password = command.Password ?? string.Empty;
        var user = email.Length == 0 ? null : await db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is not null && user.IsLockedOut(now))
        {
            var minutes = (int)Math.Ceiling((user.LockedUntil!.Value - now).TotalMinutes);
            throw new UnauthorizedException("auth.locked_out", $"Çok fazla hatalı deneme. {minutes} dakika sonra tekrar deneyin.");
        }

        if (!hasher.Verify(user?.PasswordHash, password, out var needsRehash) || user is not { IsActive: true })
        {
            if (user is not null)
            {
                user.RecordFailedLogin(now);
                await db.SaveChangesAsync(cancellationToken);
            }

            logger.LogWarning("Başarısız giriş denemesi: {Email}", email);
            throw new UnauthorizedException("auth.invalid_credentials", "E-posta veya şifre hatalı.");
        }

        if (needsRehash)
        {
            user.UpgradePasswordHash(hasher.Hash(password));
        }

        user.RecordLogin(now);
        // Süresi dolmuş eski oturum kayıtlarını bu kullanıcı girerken temizle.
        await db.RefreshTokens.Where(t => t.UserId == user.Id && t.ExpiresAt < now).ExecuteDeleteAsync(cancellationToken);
        var result = sessions.Issue(user, Guid.CreateVersion7(), now);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Giriş yapıldı: {Email}", user.Email);
        return result;
    }
}
