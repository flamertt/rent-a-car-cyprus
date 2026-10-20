namespace RentACar.Modules.Reservations.Endpoints.Requests;

internal sealed record ReturnRequest(Guid? ReturnBranchId, int OdometerKm, double FuelLevel);
