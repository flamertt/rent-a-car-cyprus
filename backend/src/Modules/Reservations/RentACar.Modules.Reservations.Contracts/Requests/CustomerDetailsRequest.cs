namespace RentACar.Modules.Reservations.Contracts.Requests;

public sealed record CustomerDetailsRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string Nationality,
    DateOnly DateOfBirth,
    string LicenseNumber,
    DateOnly LicenseIssuedOn,
    string LicenseCountry);
