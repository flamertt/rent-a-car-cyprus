using RentACar.Modules.Fleet.Contracts.Enums;

namespace RentACar.Modules.Fleet.Endpoints.Requests;

internal sealed record ChangeVehicleStatusRequest(VehicleStatus Status);
