using RentACar.Modules.Fleet.Contracts.Enums;

namespace RentACar.Modules.Fleet.Application.Queries.GetFleetSummary;

internal sealed record FleetSummaryDto(IReadOnlyDictionary<VehicleStatus, int> ByStatus, IReadOnlyList<CategoryCountDto> ByCategory);
