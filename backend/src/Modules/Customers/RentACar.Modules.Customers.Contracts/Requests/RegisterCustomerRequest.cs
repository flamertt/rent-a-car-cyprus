namespace RentACar.Modules.Customers.Contracts.Requests;

public sealed record RegisterCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string Nationality,
    DateOnly DateOfBirth,
    string LicenseNumber,
    DateOnly LicenseIssuedOn,
    string LicenseCountry,
    bool IsSimulated = false);
