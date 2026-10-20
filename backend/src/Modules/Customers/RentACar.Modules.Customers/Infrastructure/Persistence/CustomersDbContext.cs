using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Customers.Application.Abstractions;
using RentACar.Modules.Customers.Domain.Entities;
using RentACar.Shared.Infrastructure.Database;

namespace RentACar.Modules.Customers.Infrastructure.Persistence;

internal sealed class CustomersDbContext(DbContextOptions<CustomersDbContext> options) : ModuleDbContext(options), ICustomersDbContext
{
    public const string SchemaName = "customers";

    protected override string Schema => SchemaName;

    public DbSet<Customer> Customers => Set<Customer>();
}
