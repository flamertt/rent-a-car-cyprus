using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Reservations.Application.Abstractions;
using RentACar.Modules.Reservations.Contracts.Enums;
using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Reservations.Application.Queries.GetReservationTimeline;

internal sealed class GetReservationTimelineHandler(IReservationsDbContext db, IClock clock)
    : IQueryHandler<GetReservationTimelineQuery, IReadOnlyList<TimelineItemDto>>
{
    public async Task<IReadOnlyList<TimelineItemDto>> HandleAsync(GetReservationTimelineQuery query, CancellationToken cancellationToken)
    {
        var from = (query.From ?? clock.UtcNow.AddDays(-2)).ToUniversalTime();
        var to = (query.To ?? from.AddDays(14)).ToUniversalTime();
        return await db.Reservations.AsNoTracking()
            .Where(r => r.PickupAt < to && r.BlockedUntil > from)
            .Where(r => r.Status != ReservationStatus.Cancelled)
            .OrderBy(r => r.VehiclePlate).ThenBy(r => r.PickupAt)
            .Select(r => new TimelineItemDto(r.Id, r.Code, r.VehicleId, r.VehiclePlate, r.CategoryCode, r.Status,
                r.PickedUpAt ?? r.PickupAt, r.ReturnedAt ?? r.ReturnAt, r.CustomerName))
            .ToListAsync(cancellationToken);
    }
}
