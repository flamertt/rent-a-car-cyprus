using RentACar.Modules.Reservations.Contracts.Enums;
using RentACar.Shared.Abstractions.Events;

namespace RentACar.Modules.Reservations.Contracts.Events;

/// <summary>İptal ya da "gelmedi" (<see cref="Status"/>): araç takvimde serbest kalır.</summary>
public sealed record ReservationCancelled(
    Guid ReservationId, string Code, Guid VehicleId, ReservationStatus Status, DateTimeOffset OccurredAt) : IIntegrationEvent;
