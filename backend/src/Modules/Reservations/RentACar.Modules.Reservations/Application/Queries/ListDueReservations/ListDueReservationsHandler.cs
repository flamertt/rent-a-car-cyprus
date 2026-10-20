using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Reservations.Application.Abstractions;
using RentACar.Modules.Reservations.Application.Mappings;
using RentACar.Modules.Reservations.Contracts.Dtos;
using RentACar.Modules.Reservations.Contracts.Enums;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Reservations.Application.Queries.ListDueReservations;

internal sealed class ListDueReservationsHandler(IReservationsDbContext db)
    : IQueryHandler<ListDueReservationsQuery, IReadOnlyList<ReservationDto>>
{
    public async Task<IReadOnlyList<ReservationDto>> HandleAsync(ListDueReservationsQuery query, CancellationToken cancellationToken)
    {
        var due = db.Reservations.AsNoTracking().Where(r => r.Status == query.Status);
        due = query.Status switch
        {
            ReservationStatus.Confirmed => due.Where(r => r.PickupAt <= query.DueBefore).OrderBy(r => r.PickupAt),
            ReservationStatus.Active => due.Where(r => r.ReturnAt <= query.DueBefore).OrderBy(r => r.ReturnAt),
            _ => throw new DomainException("reservation.invalid_due_status", "Yalnız Confirmed ve Active için vade sorgulanır."),
        };
        var rows = await due.Take(Math.Clamp(query.Max, 1, 500)).ToListAsync(cancellationToken);
        return rows.Select(r => r.ToDto()).ToList();
    }
}
