using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentACar.Modules.Auth.Application.Abstractions;
using RentACar.Modules.Auth.Application.Dtos;
using RentACar.Modules.Auth.Application.Services;
using RentACar.Modules.Auth.Domain.Rules;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Auth.Application.Commands.RefreshSession;

/// <summary>
/// Döndürmeli yenileme. Zaten döndürülmüş bir belirteç tolerans süresinden sonra yeniden gelirse kopyalanmış
/// sayılır: ailenin (o girişin) tüm oturumları kapatılır ve kullanıcı yeniden giriş yapmak zorunda kalır.
/// </summary>
internal sealed class RefreshSessionHandler(
    IAuthDbContext db, SessionIssuer sessions, TimeProvider time, ILogger<RefreshSessionHandler> logger)
    : ICommandHandler<RefreshSessionCommand, AuthResult>
{
    public async Task<AuthResult> HandleAsync(RefreshSessionCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.RefreshToken))
        {
            throw new UnauthorizedException("auth.no_session", "Oturum bulunamadı, lütfen giriş yapın.");
        }

        var now = time.GetUtcNow();
        var hash = RefreshTokenCodec.Hash(command.RefreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken)
            ?? throw new UnauthorizedException("auth.invalid_session", "Oturum geçersiz, lütfen yeniden giriş yapın.");

        if (stored.RevokedAt is not null)
        {
            var familyAlive = await db.RefreshTokens.AnyAsync(
                t => t.FamilyId == stored.FamilyId && t.RevokedAt == null && t.ExpiresAt > now, cancellationToken);
            if (!stored.WasRotatedWithin(LoginRules.RefreshReuseGrace, now) || !familyAlive)
            {
                var revoked = await db.RefreshTokens
                    .Where(t => t.FamilyId == stored.FamilyId && t.RevokedAt == null)
                    .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), cancellationToken);
                if (revoked > 0)
                {
                    logger.LogWarning("Kullanılmış yenileme belirteci yeniden geldi; {Count} oturum kapatıldı (kullanıcı {UserId})",
                        revoked, stored.UserId);
                }

                throw new UnauthorizedException("auth.session_revoked", "Oturum sonlandırıldı, lütfen yeniden giriş yapın.");
            }
        }
        else if (stored.ExpiresAt <= now)
        {
            throw new UnauthorizedException("auth.session_expired", "Oturumun süresi doldu, lütfen yeniden giriş yapın.");
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == stored.UserId, cancellationToken);
        if (user is not { IsActive: true })
        {
            throw new UnauthorizedException("auth.invalid_session", "Oturum geçersiz, lütfen yeniden giriş yapın.");
        }

        // Tolerans içindeki eşzamanlı yenilemede eski belirteç zaten kapalı: aynı aileye kardeş belirteç verilir.
        var result = sessions.Issue(user, stored.FamilyId, now, replacing: stored.RevokedAt is null ? stored : null);
        await db.SaveChangesAsync(cancellationToken);
        return result;
    }
}
