using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Reservations.Application.Abstractions;
using RentACar.Modules.Reservations.Contracts.Enums;
using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Reservations.Application.Queries.GetReservationStats;

internal sealed class GetReservationStatsHandler(IReservationsDbContext db, IClock clock) : IQueryHandler<GetReservationStatsQuery, ReservationStatsDto>
{
    private static readonly ReservationStatus[] RevenueStatuses = [ReservationStatus.Confirmed, ReservationStatus.Active, ReservationStatus.Completed];

    public async Task<ReservationStatsDto> HandleAsync(GetReservationStatsQuery query, CancellationToken ct)
    {
        var now = clock.UtcNow;
        var since = now.AddDays(-Math.Clamp(query.Days ?? 30, 1, 365));
        var next24h = now.AddDays(1);

        // Sayımlar veritabanında (GROUP BY).
        var byStatus = await db.Reservations
            .GroupBy(r => r.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
        var byChannel = await db.Reservations
            .GroupBy(r => r.Channel)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
        var byCategory = await db.Reservations
            .Where(r => r.PickupAt >= since && RevenueStatuses.Contains(r.Status))
            .GroupBy(r => r.CategoryCode)
            .OrderByDescending(g => g.Sum(r => r.Total)) // kurucu projeksiyonundan önce: EF record üyesini SQL'e çeviremez
            .Select(g => new CategoryRevenueDto(g.Key, g.Count(), g.Sum(r => r.Total), g.Average(r => (double)r.Days)))
            .ToListAsync(ct);

        // Günlük ciro KKTC yerel gününe göre: dar pencere bellekte gruplanır (UTC gün sınırı yanıltır).
        var window = await db.Reservations.AsNoTracking()
            .Where(r => r.PickupAt >= since && r.PickupAt < next24h && RevenueStatuses.Contains(r.Status))
            .Select(r => new { r.PickupAt, r.Total })
            .ToListAsync(ct);
        var daily = window
            .GroupBy(r => KktcTime.LocalDate(r.PickupAt))
            .OrderBy(g => g.Key)
            .Select(g => new DailyRevenueDto(g.Key, g.Count(), g.Sum(x => x.Total)))
            .ToList();

        return new ReservationStatsDto(
            now,
            byStatus,
            byChannel,
            daily,
            byCategory,
            await db.Reservations.CountAsync(r => r.Status == ReservationStatus.Confirmed && r.PickupAt >= now && r.PickupAt < next24h, ct),
            await db.Reservations.CountAsync(r => r.Status == ReservationStatus.Active && r.ReturnAt >= now && r.ReturnAt < next24h, ct),
            await db.Reservations.CountAsync(r => r.Status == ReservationStatus.Active && r.ReturnAt < now, ct));
    }
}
