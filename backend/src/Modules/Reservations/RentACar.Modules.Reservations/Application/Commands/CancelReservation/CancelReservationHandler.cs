using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Reservations.Application.Abstractions;
using RentACar.Modules.Reservations.Application.Mappings;
using RentACar.Modules.Reservations.Contracts.Dtos;
using RentACar.Modules.Reservations.Contracts.Events;
using RentACar.Shared.Abstractions.Events;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Reservations.Application.Commands.CancelReservation;

internal sealed class CancelReservationHandler(IReservationsDbContext db, IEventBus bus, IClock clock)
    : ICommandHandler<CancelReservationCommand, ReservationDto>
{
    public async Task<ReservationDto> HandleAsync(CancelReservationCommand command, CancellationToken cancellationToken)
    {
        var reservation = await db.Reservations.FirstOrDefaultAsync(r => r.Id == command.ReservationId, cancellationToken)
            ?? throw new NotFoundException("Rezervasyon", command.ReservationId);
        var now = clock.UtcNow;
        reservation.Cancel(now);
        await db.SaveChangesAsync(cancellationToken);
        await bus.PublishAsync(new ReservationCancelled(reservation.Id, reservation.Code, reservation.VehicleId, reservation.Status, now), cancellationToken);
        return reservation.ToDto();
    }
}
