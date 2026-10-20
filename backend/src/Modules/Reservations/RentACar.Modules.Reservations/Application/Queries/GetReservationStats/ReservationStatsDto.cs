using RentACar.Modules.Reservations.Contracts.Enums;

namespace RentACar.Modules.Reservations.Application.Queries.GetReservationStats;

internal sealed record ReservationStatsDto(
    DateTimeOffset Now,
    IReadOnlyDictionary<ReservationStatus, int> ByStatus,
    IReadOnlyDictionary<BookingChannel, int> ByChannel,
    IReadOnlyList<DailyRevenueDto> Daily,
    IReadOnlyList<CategoryRevenueDto> ByCategory,
    int PickupsNext24h,
    int ReturnsNext24h,
    int OverdueReturns);
