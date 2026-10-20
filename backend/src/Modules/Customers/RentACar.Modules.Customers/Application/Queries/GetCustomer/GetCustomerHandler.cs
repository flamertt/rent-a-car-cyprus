using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Customers.Application.Abstractions;
using RentACar.Modules.Customers.Application.Mappings;
using RentACar.Modules.Customers.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Customers.Application.Queries.GetCustomer;

internal sealed class GetCustomerHandler(ICustomersDbContext db) : IQueryHandler<GetCustomerQuery, CustomerDto?>
{
    public async Task<CustomerDto?> HandleAsync(GetCustomerQuery query, CancellationToken cancellationToken)
    {
        var customer = await db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == query.Id, cancellationToken);
        return customer?.ToDto();
    }
}
