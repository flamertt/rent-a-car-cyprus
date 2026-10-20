using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Fleet.Application.Abstractions;
using RentACar.Modules.Fleet.Application.Mappings;
using RentACar.Modules.Fleet.Contracts.Dtos;
using RentACar.Modules.Fleet.Contracts.Enums;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Fleet.Application.Commands.ChangeVehicleStatus;

internal sealed class ChangeVehicleStatusHandler(IFleetDbContext db, IFleetLiveFeed live)
    : ICommandHandler<ChangeVehicleStatusCommand, VehicleDto>
{
    public async Task<VehicleDto> HandleAsync(ChangeVehicleStatusCommand command, CancellationToken cancellationToken)
    {
        var vehicle = await db.Vehicles.Include(v => v.Category).FirstOrDefaultAsync(v => v.Id == command.VehicleId, cancellationToken)
            ?? throw new NotFoundException("Araç", command.VehicleId);
        vehicle.ChangeStatus(command.Status);
        await db.SaveChangesAsync(cancellationToken);
        if (vehicle.Status == VehicleStatus.Retired)
        {
            live.Remove(vehicle.Id);
        }
        else
        {
            live.Upsert(vehicle.ToLive());
        }

        return vehicle.ToDto();
    }
}
