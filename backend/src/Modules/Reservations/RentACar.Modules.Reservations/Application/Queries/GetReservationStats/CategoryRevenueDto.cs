namespace RentACar.Modules.Reservations.Application.Queries.GetReservationStats;

internal sealed record CategoryRevenueDto(string CategoryCode, int Reservations, decimal Revenue, double AvgDays);
