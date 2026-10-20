using RentACar.Modules.Reservations.Contracts.Dtos;
using RentACar.Modules.Reservations.Contracts.Enums;
using RentACar.Shared.Abstractions.Common;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Reservations.Application.Queries.ListReservations;

internal sealed record ListReservationsQuery(
    ReservationStatus? Status,
    BookingChannel? Channel,
    Guid? BranchId,
    DateTimeOffset? From,
    DateTimeOffset? To,
    string? Search,
    int? Page,
    int? PageSize) : IQuery<PagedResult<ReservationDto>>;
