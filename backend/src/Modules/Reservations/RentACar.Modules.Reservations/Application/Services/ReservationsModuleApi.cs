using RentACar.Modules.Reservations.Application.Commands.CancelReservation;
using RentACar.Modules.Reservations.Application.Commands.CreateReservation;
using RentACar.Modules.Reservations.Application.Commands.PickUpReservation;
using RentACar.Modules.Reservations.Application.Commands.ReturnReservation;
using RentACar.Modules.Reservations.Application.Queries.ListDueReservations;
using RentACar.Modules.Reservations.Contracts;
using RentACar.Modules.Reservations.Contracts.Dtos;
using RentACar.Modules.Reservations.Contracts.Enums;
using RentACar.Modules.Reservations.Contracts.Requests;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Reservations.Application.Services;

internal sealed class ReservationsModuleApi(IDispatcher dispatcher) : IReservationsApi
{
    public Task<ReservationDto> CreateAsync(CreateReservationRequest request, CancellationToken cancellationToken = default) =>
        dispatcher.SendAsync(new CreateReservationCommand(request), cancellationToken);

    public Task<ReservationDto> PickUpAsync(Guid reservationId, int odometerKm, double fuelLevel, CancellationToken cancellationToken = default) =>
        dispatcher.SendAsync(new PickUpReservationCommand(reservationId, odometerKm, fuelLevel), cancellationToken);

    public Task<ReservationDto> ReturnAsync(Guid reservationId, Guid? returnBranchId, int odometerKm, double fuelLevel, CancellationToken cancellationToken = default) =>
        dispatcher.SendAsync(new ReturnReservationCommand(reservationId, returnBranchId, odometerKm, fuelLevel), cancellationToken);

    public Task<ReservationDto> CancelAsync(Guid reservationId, CancellationToken cancellationToken = default) =>
        dispatcher.SendAsync(new CancelReservationCommand(reservationId), cancellationToken);

    public Task<IReadOnlyList<ReservationDto>> ListDueAsync(ReservationStatus status, DateTimeOffset dueBefore, int max, CancellationToken cancellationToken = default) =>
        dispatcher.QueryAsync(new ListDueReservationsQuery(status, dueBefore, max), cancellationToken);
}
