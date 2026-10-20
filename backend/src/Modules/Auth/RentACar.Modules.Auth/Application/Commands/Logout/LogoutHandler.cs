using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Auth.Application.Abstractions;
using RentACar.Modules.Auth.Application.Services;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Auth.Application.Commands.Logout;

internal sealed class LogoutHandler(IAuthDbContext db, TimeProvider time) : ICommandHandler<LogoutCommand, Unit>
{
    public async Task<Unit> HandleAsync(LogoutCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.RefreshToken))
        {
            return Unit.Value;
        }

        var hash = RefreshTokenCodec.Hash(command.RefreshToken);
        var familyId = await db.RefreshTokens.Where(t => t.TokenHash == hash).Select(t => (Guid?)t.FamilyId).FirstOrDefaultAsync(cancellationToken);
        if (familyId is { } family)
        {
            var now = time.GetUtcNow();
            await db.RefreshTokens
                .Where(t => t.FamilyId == family && t.RevokedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), cancellationToken);
        }

        return Unit.Value;
    }
}
