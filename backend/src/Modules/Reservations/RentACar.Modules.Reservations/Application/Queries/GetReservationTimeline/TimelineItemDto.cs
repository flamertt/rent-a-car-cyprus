using RentACar.Modules.Reservations.Contracts.Enums;

namespace RentACar.Modules.Reservations.Application.Queries.GetReservationTimeline;

internal sealed record TimelineItemDto(
    Guid Id,
    string Code,
    Guid VehicleId,
    string VehiclePlate,
    string CategoryCode,
    ReservationStatus Status,
    DateTimeOffset From,
    DateTimeOffset To,
    string CustomerName);
