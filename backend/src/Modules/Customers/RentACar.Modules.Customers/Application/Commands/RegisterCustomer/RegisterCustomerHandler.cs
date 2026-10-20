using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Customers.Application.Abstractions;
using RentACar.Modules.Customers.Application.Mappings;
using RentACar.Modules.Customers.Contracts.Dtos;
using RentACar.Modules.Customers.Domain.Entities;
using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Customers.Application.Commands.RegisterCustomer;

internal sealed class RegisterCustomerHandler(ICustomersDbContext db, IClock clock) : ICommandHandler<RegisterCustomerCommand, CustomerDto>
{
    public async Task<CustomerDto> HandleAsync(RegisterCustomerCommand command, CancellationToken cancellationToken)
    {
        var c = command.Request;
        var email = Customer.NormalizeEmail(c.Email);
        var customer = await db.Customers.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
        if (customer is null)
        {
            customer = Customer.Register(c.FirstName, c.LastName, c.Email, c.Phone, c.Nationality, c.DateOfBirth,
                c.LicenseNumber, c.LicenseIssuedOn, c.LicenseCountry, c.IsSimulated, clock.UtcNow);
            db.Customers.Add(customer);
        }
        else
        {
            customer.Update(c.FirstName, c.LastName, c.Phone, c.Nationality, c.DateOfBirth,
                c.LicenseNumber, c.LicenseIssuedOn, c.LicenseCountry, clock.UtcNow);
        }

        await db.SaveChangesAsync(cancellationToken);
        return customer.ToDto();
    }
}
