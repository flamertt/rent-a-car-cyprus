using RentACar.Modules.Reservations.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Reservations.Application.Commands.CancelReservation;

internal sealed record CancelReservationCommand(Guid ReservationId) : ICommand<ReservationDto>;
