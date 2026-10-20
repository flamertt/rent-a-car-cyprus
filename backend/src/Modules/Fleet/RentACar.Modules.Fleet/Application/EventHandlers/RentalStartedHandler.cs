using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Fleet.Application.Abstractions;
using RentACar.Modules.Fleet.Application.Mappings;
using RentACar.Modules.Reservations.Contracts.Events;
using RentACar.Shared.Abstractions.Events;
using RentACar.Shared.Abstractions.Exceptions;

namespace RentACar.Modules.Fleet.Application.EventHandlers;

/// <summary>Kiralama başladı (Reservations) → araç müşteride.</summary>
internal sealed class RentalStartedHandler(IFleetDbContext db, IFleetLiveFeed live) : IIntegrationEventHandler<RentalStarted>
{
    public async Task HandleAsync(RentalStarted e, CancellationToken cancellationToken)
    {
        var vehicle = await db.Vehicles.Include(v => v.Category).FirstOrDefaultAsync(v => v.Id == e.VehicleId, cancellationToken)
            ?? throw new NotFoundException("Araç", e.VehicleId);
        vehicle.StartRental(e.OdometerKm, e.FuelLevel, e.OccurredAt);
        await db.SaveChangesAsync(cancellationToken);
        live.Upsert(vehicle.ToLive());
    }
}
