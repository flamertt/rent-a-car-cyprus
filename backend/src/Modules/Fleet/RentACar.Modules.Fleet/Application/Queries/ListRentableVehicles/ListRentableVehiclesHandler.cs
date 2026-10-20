using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Fleet.Application.Abstractions;
using RentACar.Modules.Fleet.Contracts.Dtos;
using RentACar.Modules.Fleet.Contracts.Enums;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Fleet.Application.Queries.ListRentableVehicles;

internal sealed class ListRentableVehiclesHandler(IFleetDbContext db)
    : IQueryHandler<ListRentableVehiclesQuery, IReadOnlyList<RentableVehicleDto>>
{
    public async Task<IReadOnlyList<RentableVehicleDto>> HandleAsync(ListRentableVehiclesQuery query, CancellationToken cancellationToken)
    {
        var vehicles = db.Vehicles.AsNoTracking()
            .Where(v => v.Status != VehicleStatus.Maintenance && v.Status != VehicleStatus.Retired);
        if (query.CategoryId is { } categoryId)
        {
            vehicles = vehicles.Where(v => v.CategoryId == categoryId);
        }

        return await vehicles
            .OrderBy(v => v.OdometerKm)
            .Select(v => new RentableVehicleDto(v.Id, v.CategoryId, v.Plate, v.BranchId, v.OdometerKm))
            .ToListAsync(cancellationToken);
    }
}
