using RentACar.Modules.Reservations.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Reservations.Application.Queries.GetReservation;

internal sealed record GetReservationQuery(Guid Id) : IQuery<ReservationDto?>;
