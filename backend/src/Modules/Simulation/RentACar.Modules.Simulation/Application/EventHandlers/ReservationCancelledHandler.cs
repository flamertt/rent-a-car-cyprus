using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Reservations.Contracts.Enums;
using RentACar.Modules.Reservations.Contracts.Events;
using RentACar.Modules.Simulation.Application.Abstractions;
using RentACar.Modules.Simulation.Application.Engine;
using RentACar.Modules.Simulation.Domain.Enums;
using RentACar.Shared.Abstractions.Events;

namespace RentACar.Modules.Simulation.Application.EventHandlers;

/// <summary>
/// Simülasyon dışından gelen kapanış (admin iptali, gelmedi) henüz yola çıkmamış teslimatı sonlandırır;
/// yoldaki teslimat aracı varınca şubede kalır, transfer onu sonra ofise taşır.
/// </summary>
internal sealed class ReservationCancelledHandler(ISimulationDbContext db, EventLog log) : IIntegrationEventHandler<ReservationCancelled>
{
    public async Task HandleAsync(ReservationCancelled e, CancellationToken cancellationToken)
    {
        var waiting = await db.Trips
            .Where(t => t.ReservationId == e.ReservationId && t.State == TripState.Waiting && t.LegStartedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var trip in waiting)
        {
            trip.Finish(e.OccurredAt);
        }

        var noShow = e.Status == ReservationStatus.NoShow;
        log.Write(e.OccurredAt, noShow ? "noshow" : "cancelled",
            noShow ? $"{e.Code}: müşteri gelmedi, rezervasyon kapandı" : $"{e.Code} iptal edildi", e.VehicleId, e.Code);
        await db.SaveChangesAsync(cancellationToken);
    }
}
