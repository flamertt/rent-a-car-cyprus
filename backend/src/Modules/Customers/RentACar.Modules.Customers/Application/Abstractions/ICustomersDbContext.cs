using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Customers.Domain.Entities;

namespace RentACar.Modules.Customers.Application.Abstractions;

internal interface ICustomersDbContext
{
    DbSet<Customer> Customers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
