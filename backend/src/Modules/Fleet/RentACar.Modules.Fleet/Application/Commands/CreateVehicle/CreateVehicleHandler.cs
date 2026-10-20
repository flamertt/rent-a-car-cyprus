using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Branches.Contracts;
using RentACar.Modules.Fleet.Application.Abstractions;
using RentACar.Modules.Fleet.Application.Mappings;
using RentACar.Modules.Fleet.Application.Services;
using RentACar.Modules.Fleet.Contracts.Dtos;
using RentACar.Modules.Fleet.Domain.Entities;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Fleet.Application.Commands.CreateVehicle;

internal sealed class CreateVehicleHandler(IFleetDbContext db, IBranchesApi branches, ParkingAllocator parking, IFleetLiveFeed live)
    : ICommandHandler<CreateVehicleCommand, VehicleDto>
{
    public async Task<VehicleDto> HandleAsync(CreateVehicleCommand c, CancellationToken cancellationToken)
    {
        var category = await db.Categories.FirstOrDefaultAsync(x => x.Id == c.CategoryId, cancellationToken)
            ?? throw new NotFoundException("Araç sınıfı", c.CategoryId);
        var branch = await branches.GetAsync(c.BranchId, cancellationToken) ?? throw new NotFoundException("Şube", c.BranchId);
        var plate = Vehicle.NormalizePlate(c.Plate);
        if (await db.Vehicles.AnyAsync(v => v.Plate == plate, cancellationToken))
        {
            throw new ConflictException("vehicle.plate_taken", $"{plate} plakası filoda kayıtlı.");
        }

        var slot = await parking.NextFreeSlotAsync(branch.Id, cancellationToken);
        var vehicle = Vehicle.Create(plate, c.Make, c.Model, c.Year, c.Color, category, branch.Id, branch.Location, slot, c.OdometerKm);
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync(cancellationToken);
        live.Upsert(vehicle.ToLive());
        return vehicle.ToDto();
    }
}
