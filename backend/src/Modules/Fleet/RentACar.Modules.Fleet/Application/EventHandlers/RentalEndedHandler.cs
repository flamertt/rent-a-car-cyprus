using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Branches.Contracts;
using RentACar.Modules.Fleet.Application.Abstractions;
using RentACar.Modules.Fleet.Application.Mappings;
using RentACar.Modules.Fleet.Application.Services;
using RentACar.Modules.Reservations.Contracts.Events;
using RentACar.Shared.Abstractions.Events;
using RentACar.Shared.Abstractions.Exceptions;

namespace RentACar.Modules.Fleet.Application.EventHandlers;

/// <summary>Kiralama bitti (Reservations) → araç iade şubesinin otoparkında.</summary>
internal sealed class RentalEndedHandler(IFleetDbContext db, IBranchesApi branches, ParkingAllocator parking, IFleetLiveFeed live)
    : IIntegrationEventHandler<RentalEnded>
{
    public async Task HandleAsync(RentalEnded e, CancellationToken cancellationToken)
    {
        var vehicle = await db.Vehicles.Include(v => v.Category).FirstOrDefaultAsync(v => v.Id == e.VehicleId, cancellationToken)
            ?? throw new NotFoundException("Araç", e.VehicleId);
        var branch = await branches.GetAsync(e.ReturnBranchId, cancellationToken)
            ?? throw new NotFoundException("Şube", e.ReturnBranchId);
        var slot = await parking.NextFreeSlotAsync(branch.Id, cancellationToken);
        vehicle.EndRental(branch.Id, branch.Location, slot, e.OdometerKm, e.FuelLevel, e.OccurredAt);
        await db.SaveChangesAsync(cancellationToken);
        live.Upsert(vehicle.ToLive());
    }
}
