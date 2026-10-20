using RentACar.Shared.Abstractions.Events;

namespace RentACar.Modules.Reservations.Contracts.Events;

public sealed record RentalStarted(
    Guid ReservationId, Guid VehicleId, Guid PickupBranchId, int OdometerKm, double FuelLevel, DateTimeOffset OccurredAt) : IIntegrationEvent;
