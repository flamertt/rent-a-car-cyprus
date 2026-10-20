using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RentACar.Modules.Branches.Contracts;
using RentACar.Modules.Reservations.Application.Abstractions;
using RentACar.Modules.Reservations.Application.Mappings;
using RentACar.Modules.Reservations.Contracts.Dtos;
using RentACar.Modules.Reservations.Contracts.Events;
using RentACar.Modules.Reservations.Domain.Rules;
using RentACar.Shared.Abstractions.Events;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Reservations.Application.Commands.ReturnReservation;

internal sealed class ReturnReservationHandler(
    IReservationsDbContext db, IBranchesApi branches, IEventBus bus, IClock clock, IOptions<ReservationRules> rules)
    : ICommandHandler<ReturnReservationCommand, ReservationDto>
{
    public async Task<ReservationDto> HandleAsync(ReturnReservationCommand command, CancellationToken cancellationToken)
    {
        var reservation = await db.Reservations.FirstOrDefaultAsync(r => r.Id == command.ReservationId, cancellationToken)
            ?? throw new NotFoundException("Rezervasyon", command.ReservationId);
        var branchId = command.ReturnBranchId ?? reservation.ReturnBranchId;
        _ = await branches.GetAsync(branchId, cancellationToken) ?? throw new NotFoundException("İade şubesi", branchId);

        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        reservation.Return(clock.UtcNow, branchId, command.OdometerKm, command.FuelLevel, rules.Value);
        await db.SaveChangesAsync(cancellationToken);
        await bus.PublishAsync(new RentalEnded(
            reservation.Id, reservation.VehicleId, branchId, command.OdometerKm, command.FuelLevel, reservation.ReturnedAt!.Value), cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return reservation.ToDto();
    }
}
