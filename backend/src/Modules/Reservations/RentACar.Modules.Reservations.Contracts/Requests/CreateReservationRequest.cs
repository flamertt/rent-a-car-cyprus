using RentACar.Modules.Reservations.Contracts.Enums;

namespace RentACar.Modules.Reservations.Contracts.Requests;

/// <summary>Mevcut müşteri için <see cref="CustomerId"/>, yeni müşteri için <see cref="Customer"/> verilir.</summary>
public sealed record CreateReservationRequest(
    Guid? CustomerId,
    CustomerDetailsRequest? Customer,
    Guid CategoryId,
    Guid PickupBranchId,
    Guid ReturnBranchId,
    DateTimeOffset PickupAt,
    DateTimeOffset ReturnAt,
    IReadOnlyList<ReservationExtraRequest>? Extras,
    BookingChannel Channel = BookingChannel.Simulation);
