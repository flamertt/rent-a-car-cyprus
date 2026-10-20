using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Fleet.Application.Abstractions;
using RentACar.Modules.Fleet.Application.Mappings;
using RentACar.Modules.Fleet.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Fleet.Application.Queries.GetVehicle;

internal sealed class GetVehicleHandler(IFleetDbContext db) : IQueryHandler<GetVehicleQuery, VehicleDto?>
{
    public async Task<VehicleDto?> HandleAsync(GetVehicleQuery query, CancellationToken cancellationToken)
    {
        var vehicle = await db.Vehicles.AsNoTracking().Include(v => v.Category).FirstOrDefaultAsync(v => v.Id == query.Id, cancellationToken);
        return vehicle?.ToDto();
    }
}
