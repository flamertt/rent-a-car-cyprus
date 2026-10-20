namespace RentACar.Modules.Customers.Contracts.Dtos;

public sealed record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string Nationality,
    DateOnly DateOfBirth,
    string LicenseNumber,
    DateOnly LicenseIssuedOn,
    string LicenseCountry,
    bool IsSimulated,
    DateTimeOffset CreatedAt)
{
    public string FullName => $"{FirstName} {LastName}";

    public int AgeOn(DateOnly date) => YearsBetween(DateOfBirth, date);

    public int LicenseYearsOn(DateOnly date) => YearsBetween(LicenseIssuedOn, date);

    private static int YearsBetween(DateOnly from, DateOnly to)
    {
        var years = to.Year - from.Year;
        return from.AddYears(years) > to ? years - 1 : years;
    }
}
