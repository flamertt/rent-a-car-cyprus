using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Branches.Application.Abstractions;
using RentACar.Modules.Branches.Domain.Entities;
using RentACar.Shared.Abstractions.Exceptions;

namespace RentACar.Modules.Branches.Application.Services;

internal static class BranchCodeGuard
{
    public static async Task EnsureFreeAsync(IBranchesDbContext db, string code, Guid? exceptId, CancellationToken ct)
    {
        var normalized = Branch.NormalizeCode(code);
        if (await db.Branches.AnyAsync(b => b.Code == normalized && b.Id != exceptId, ct))
        {
            throw new ConflictException("branch.code_taken", $"'{normalized}' kodu başka bir şubede kullanılıyor.");
        }
    }
}
