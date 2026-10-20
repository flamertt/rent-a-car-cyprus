using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RentACar.Modules.Fleet.Contracts;
using RentACar.Modules.Reservations.Application.Abstractions;
using RentACar.Modules.Reservations.Application.Mappings;
using RentACar.Modules.Reservations.Contracts.Dtos;
using RentACar.Modules.Reservations.Contracts.Events;
using RentACar.Modules.Reservations.Domain.Rules;
using RentACar.Shared.Abstractions.Events;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Reservations.Application.Commands.PickUpReservation;

/// <summary>
/// Önce kendi işlemimiz açılır; Fleet olay işleyicisi başarılı olursa commit edilir. İşleyici hata verirse
/// (örn. araç bakımda) rezervasyon değişmez.
/// </summary>
internal sealed class PickUpReservationHandler(
    IReservationsDbContext db, IFleetApi fleet, IEventBus bus, IClock clock, IOptions<ReservationRules> rules)
    : ICommandHandler<PickUpReservationCommand, ReservationDto>
{
    public async Task<ReservationDto> HandleAsync(PickUpReservationCommand command, CancellationToken cancellationToken)
    {
        var reservation = await db.Reservations.FirstOrDefaultAsync(r => r.Id == command.ReservationId, cancellationToken)
            ?? throw new NotFoundException("Rezervasyon", command.ReservationId);
        var vehicle = await fleet.GetVehicleAsync(reservation.VehicleId, cancellationToken)
            ?? throw new NotFoundException("Araç", reservation.VehicleId);
        var odometer = command.OdometerKm ?? vehicle.OdometerKm;
        var fuel = command.FuelLevel ?? vehicle.FuelLevel;

        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        reservation.PickUp(clock.UtcNow, odometer, fuel, rules.Value);
        await db.SaveChangesAsync(cancellationToken);
        await bus.PublishAsync(new RentalStarted(
            reservation.Id, reservation.VehicleId, reservation.PickupBranchId, odometer, fuel, reservation.PickedUpAt!.Value), cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return reservation.ToDto();
    }
}
