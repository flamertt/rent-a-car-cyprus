namespace RentACar.Modules.Reservations.Application.Queries.GetReservationStats;

internal sealed record DailyRevenueDto(DateOnly Date, int Reservations, decimal Revenue);
