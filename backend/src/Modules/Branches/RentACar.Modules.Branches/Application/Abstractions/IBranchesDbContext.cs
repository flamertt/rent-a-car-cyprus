using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Branches.Domain.Entities;

namespace RentACar.Modules.Branches.Application.Abstractions;

/// <summary>Uygulama katmanının gördüğü kalıcılık yüzü (LINQ için DbSet + iş birimi).</summary>
internal interface IBranchesDbContext
{
    DbSet<Branch> Branches { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
