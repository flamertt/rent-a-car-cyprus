namespace RentACar.Modules.Reservations.Endpoints.Requests;

internal sealed record PickUpRequest(int? OdometerKm, double? FuelLevel);
