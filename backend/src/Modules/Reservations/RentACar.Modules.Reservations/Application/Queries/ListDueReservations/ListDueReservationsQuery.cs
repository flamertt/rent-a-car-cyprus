using RentACar.Modules.Reservations.Contracts.Dtos;
using RentACar.Modules.Reservations.Contracts.Enums;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Reservations.Application.Queries.ListDueReservations;

/// <summary>Confirmed: teslimi <paramref name="DueBefore"/>'dan önce olanlar; Active: iadesi.</summary>
internal sealed record ListDueReservationsQuery(ReservationStatus Status, DateTimeOffset DueBefore, int Max)
    : IQuery<IReadOnlyList<ReservationDto>>;
