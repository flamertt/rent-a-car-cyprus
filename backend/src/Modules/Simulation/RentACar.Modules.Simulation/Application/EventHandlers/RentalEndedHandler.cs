using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Reservations.Contracts.Events;
using RentACar.Modules.Simulation.Application.Abstractions;
using RentACar.Modules.Simulation.Domain.Enums;
using RentACar.Shared.Abstractions.Events;

namespace RentACar.Modules.Simulation.Application.EventHandlers;

/// <summary>Elle (admin) iade alınan kiralamanın simülasyon yolculuğunu kapatır.</summary>
internal sealed class RentalEndedHandler(ISimulationDbContext db) : IIntegrationEventHandler<RentalEnded>
{
    public async Task HandleAsync(RentalEnded e, CancellationToken cancellationToken)
    {
        var open = await db.Trips
            .Where(t => t.ReservationId == e.ReservationId && t.Kind == TripKind.Rental && t.State != TripState.Finished)
            .ToListAsync(cancellationToken);
        foreach (var trip in open)
        {
            trip.Finish(e.OccurredAt);
        }

        if (open.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
