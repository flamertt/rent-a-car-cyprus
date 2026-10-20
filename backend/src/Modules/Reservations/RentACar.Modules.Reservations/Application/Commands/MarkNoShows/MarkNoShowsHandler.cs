using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RentACar.Modules.Reservations.Application.Abstractions;
using RentACar.Modules.Reservations.Contracts.Enums;
using RentACar.Modules.Reservations.Contracts.Events;
using RentACar.Modules.Reservations.Domain.Rules;
using RentACar.Shared.Abstractions.Events;
using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Reservations.Application.Commands.MarkNoShows;

internal sealed class MarkNoShowsHandler(IReservationsDbContext db, IEventBus bus, IClock clock, IOptions<ReservationRules> rules)
    : ICommandHandler<MarkNoShowsCommand, int>
{
    public async Task<int> HandleAsync(MarkNoShowsCommand command, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var threshold = now.AddMinutes(-rules.Value.NoShowAfterMinutes);
        var overdue = await db.Reservations
            .Where(r => r.Status == ReservationStatus.Confirmed && r.PickupAt < threshold)
            .OrderBy(r => r.PickupAt)
            .Take(100)
            .ToListAsync(cancellationToken);
        foreach (var reservation in overdue)
        {
            reservation.MarkNoShow(now);
        }

        await db.SaveChangesAsync(cancellationToken);
        foreach (var reservation in overdue)
        {
            await bus.PublishAsync(new ReservationCancelled(reservation.Id, reservation.Code, reservation.VehicleId, reservation.Status, now), cancellationToken);
        }

        return overdue.Count;
    }
}
