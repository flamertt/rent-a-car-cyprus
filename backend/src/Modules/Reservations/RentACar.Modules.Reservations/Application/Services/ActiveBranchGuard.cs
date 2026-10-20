using RentACar.Modules.Branches.Contracts;
using RentACar.Shared.Abstractions.Exceptions;

namespace RentACar.Modules.Reservations.Application.Services;

internal sealed class ActiveBranchGuard(IBranchesApi branches)
{
    public async Task EnsureActiveAsync(Guid pickupBranchId, Guid returnBranchId, CancellationToken ct)
    {
        foreach (var id in new[] { pickupBranchId, returnBranchId }.Distinct())
        {
            var branch = await branches.GetAsync(id, ct) ?? throw new NotFoundException("Şube", id);
            if (!branch.IsActive)
            {
                throw new DomainException("reservation.branch_inactive", $"{branch.Name} şu an hizmet vermiyor.");
            }
        }
    }
}
