using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Reservations.Application.Queries.GetReservationStats;

/// <summary>Pano: son <paramref name="Days"/> gün (1–365, varsayılan 30).</summary>
internal sealed record GetReservationStatsQuery(int? Days) : IQuery<ReservationStatsDto>;
