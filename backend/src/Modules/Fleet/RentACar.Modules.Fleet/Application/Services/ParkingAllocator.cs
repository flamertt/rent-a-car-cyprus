using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Fleet.Application.Abstractions;
using RentACar.Modules.Fleet.Domain.Services;

namespace RentACar.Modules.Fleet.Application.Services;

/// <summary>Şubeye park eden araca o şubede park etmiş araçların tutmadığı ilk otopark yerini verir.</summary>
internal sealed class ParkingAllocator(IFleetDbContext db)
{
    public async Task<int> NextFreeSlotAsync(Guid branchId, CancellationToken cancellationToken)
    {
        var occupied = await db.Vehicles.Where(v => v.BranchId == branchId).Select(v => v.ParkingSlot).ToListAsync(cancellationToken);
        return ParkingLot.FirstFree(occupied);
    }
}
