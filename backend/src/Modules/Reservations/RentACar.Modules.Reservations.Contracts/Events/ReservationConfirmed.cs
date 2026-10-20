using RentACar.Shared.Abstractions.Events;

namespace RentACar.Modules.Reservations.Contracts.Events;

public sealed record ReservationConfirmed(
    Guid ReservationId, Guid VehicleId, Guid PickupBranchId, DateTimeOffset PickupAt, DateTimeOffset OccurredAt) : IIntegrationEvent;
