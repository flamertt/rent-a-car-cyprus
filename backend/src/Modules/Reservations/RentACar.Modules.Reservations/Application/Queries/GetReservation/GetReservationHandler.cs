using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Reservations.Application.Abstractions;
using RentACar.Modules.Reservations.Application.Mappings;
using RentACar.Modules.Reservations.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Reservations.Application.Queries.GetReservation;

internal sealed class GetReservationHandler(IReservationsDbContext db) : IQueryHandler<GetReservationQuery, ReservationDto?>
{
    public async Task<ReservationDto?> HandleAsync(GetReservationQuery query, CancellationToken cancellationToken)
    {
        var reservation = await db.Reservations.AsNoTracking().FirstOrDefaultAsync(r => r.Id == query.Id, cancellationToken);
        return reservation?.ToDto();
    }
}
