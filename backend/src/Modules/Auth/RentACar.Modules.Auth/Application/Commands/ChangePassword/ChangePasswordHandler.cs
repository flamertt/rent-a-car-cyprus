using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Auth.Application.Abstractions;
using RentACar.Modules.Auth.Domain.Rules;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Auth.Application.Commands.ChangePassword;

internal sealed class ChangePasswordHandler(IAuthDbContext db, IPasswordHasher hasher, TimeProvider time)
    : ICommandHandler<ChangePasswordCommand, Unit>
{
    public async Task<Unit> HandleAsync(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken)
            ?? throw new UnauthorizedException("auth.invalid_session", "Oturum geçersiz, lütfen yeniden giriş yapın.");

        if (!hasher.Verify(user.PasswordHash, command.CurrentPassword ?? string.Empty, out _))
        {
            throw new DomainException("auth.wrong_password", "Mevcut şifre hatalı.");
        }

        LoginRules.EnsurePasswordIsAcceptable(command.NewPassword);
        if (command.NewPassword == command.CurrentPassword)
        {
            throw new DomainException("auth.same_password", "Yeni şifre mevcut şifreyle aynı olamaz.");
        }

        var now = time.GetUtcNow();
        user.ChangePassword(hasher.Hash(command.NewPassword!), now);
        await db.RefreshTokens
            .Where(t => t.UserId == user.Id && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
