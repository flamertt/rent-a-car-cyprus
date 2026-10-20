using RentACar.Shared.Abstractions.Events;

namespace RentACar.Modules.Reservations.Contracts.Events;

public sealed record RentalEnded(
    Guid ReservationId, Guid VehicleId, Guid ReturnBranchId, int OdometerKm, double FuelLevel, DateTimeOffset OccurredAt) : IIntegrationEvent;
