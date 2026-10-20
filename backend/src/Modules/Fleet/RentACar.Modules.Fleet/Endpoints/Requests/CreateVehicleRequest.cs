namespace RentACar.Modules.Fleet.Endpoints.Requests;

internal sealed record CreateVehicleRequest(
    string Plate, string Make, string Model, int Year, string Color, Guid CategoryId, Guid BranchId, int OdometerKm);
