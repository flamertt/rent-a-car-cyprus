using RentACar.Modules.Reservations.Contracts.Enums;

namespace RentACar.Modules.Reservations.Contracts.Dtos;

public sealed record ReservationDto(
    Guid Id,
    string Code,
    ReservationStatus Status,
    BookingChannel Channel,
    Guid CustomerId,
    string CustomerName,
    Guid CategoryId,
    string CategoryCode,
    Guid VehicleId,
    string VehiclePlate,
    Guid PickupBranchId,
    Guid ReturnBranchId,
    DateTimeOffset PickupAt,
    DateTimeOffset ReturnAt,
    DateTimeOffset? PickedUpAt,
    DateTimeOffset? ReturnedAt,
    int Days,
    decimal Total,
    string Currency,
    IReadOnlyList<PriceLineDto> Lines,
    DateTimeOffset CreatedAt);
