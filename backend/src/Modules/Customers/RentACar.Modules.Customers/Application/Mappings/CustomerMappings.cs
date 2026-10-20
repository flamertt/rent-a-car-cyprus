using RentACar.Modules.Customers.Contracts.Dtos;
using RentACar.Modules.Customers.Domain.Entities;

namespace RentACar.Modules.Customers.Application.Mappings;

internal static class CustomerMappings
{
    public static CustomerDto ToDto(this Customer c) => new(
        c.Id, c.FirstName, c.LastName, c.Email, c.Phone, c.Nationality, c.DateOfBirth,
        c.LicenseNumber, c.LicenseIssuedOn, c.LicenseCountry, c.IsSimulated, c.CreatedAt);
}
