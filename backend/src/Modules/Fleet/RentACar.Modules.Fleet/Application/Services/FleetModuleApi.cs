using RentACar.Modules.Fleet.Application.Commands.ReportTelemetry;
using RentACar.Modules.Fleet.Application.Commands.SetVehicleInTransit;
using RentACar.Modules.Fleet.Application.Queries.GetCategory;
using RentACar.Modules.Fleet.Application.Queries.GetVehicle;
using RentACar.Modules.Fleet.Application.Queries.ListCategories;
using RentACar.Modules.Fleet.Application.Queries.ListRentableVehicles;
using RentACar.Modules.Fleet.Contracts;
using RentACar.Modules.Fleet.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Fleet.Application.Services;

internal sealed class FleetModuleApi(IDispatcher dispatcher) : IFleetApi
{
    public Task<IReadOnlyList<VehicleCategoryDto>> ListCategoriesAsync(CancellationToken cancellationToken = default) =>
        dispatcher.QueryAsync(new ListCategoriesQuery(), cancellationToken);

    public Task<VehicleCategoryDto?> GetCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default) =>
        dispatcher.QueryAsync(new GetCategoryQuery(categoryId), cancellationToken);

    public Task<VehicleDto?> GetVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default) =>
        dispatcher.QueryAsync(new GetVehicleQuery(vehicleId), cancellationToken);

    public Task<IReadOnlyList<RentableVehicleDto>> ListRentableVehiclesAsync(Guid? categoryId = null, CancellationToken cancellationToken = default) =>
        dispatcher.QueryAsync(new ListRentableVehiclesQuery(categoryId), cancellationToken);

    public Task ReportTelemetryAsync(IReadOnlyList<VehicleTelemetryDto> telemetry, CancellationToken cancellationToken = default) =>
        dispatcher.SendAsync(new ReportTelemetryCommand(telemetry), cancellationToken);

    public Task SetInTransitAsync(Guid vehicleId, bool inTransit, Guid? arrivedBranchId, CancellationToken cancellationToken = default) =>
        dispatcher.SendAsync(new SetVehicleInTransitCommand(vehicleId, inTransit, arrivedBranchId), cancellationToken);
}
