using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Branches.Contracts;
using RentACar.Modules.Fleet.Application.Abstractions;
using RentACar.Modules.Fleet.Application.Mappings;
using RentACar.Modules.Fleet.Application.Services;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Fleet.Application.Commands.SetVehicleInTransit;

internal sealed class SetVehicleInTransitHandler(
    IFleetDbContext db, IBranchesApi branches, ParkingAllocator parking, IFleetLiveFeed live, IClock clock)
    : ICommandHandler<SetVehicleInTransitCommand, Unit>
{
    public async Task<Unit> HandleAsync(SetVehicleInTransitCommand command, CancellationToken cancellationToken)
    {
        var vehicle = await db.Vehicles.Include(v => v.Category).FirstOrDefaultAsync(v => v.Id == command.VehicleId, cancellationToken)
            ?? throw new NotFoundException("Araç", command.VehicleId);
        if (command.InTransit)
        {
            vehicle.SetInTransit(clock.UtcNow);
        }
        else
        {
            var branchId = command.ArrivedBranchId ?? throw new DomainException("vehicle.branch_required", "Varış şubesi zorunlu.");
            var branch = await branches.GetAsync(branchId, cancellationToken) ?? throw new NotFoundException("Şube", branchId);
            vehicle.Arrive(branch.Id, branch.Location, await parking.NextFreeSlotAsync(branch.Id, cancellationToken), clock.UtcNow);
        }

        await db.SaveChangesAsync(cancellationToken);
        live.Upsert(vehicle.ToLive());
        return Unit.Value;
    }
}
