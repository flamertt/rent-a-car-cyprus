using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Fleet.Domain.Entities;

namespace RentACar.Modules.Fleet.Application.Abstractions;

internal interface IFleetDbContext
{
    DbSet<VehicleCategory> Categories { get; }

    DbSet<Vehicle> Vehicles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
