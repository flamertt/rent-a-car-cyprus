using RentACar.Shared.Abstractions.Exceptions;

namespace RentACar.Modules.Customers.Domain.Entities;

internal sealed class Customer
{
    private Customer()
    {
    }

    public Guid Id { get; private set; }

    public string FirstName { get; private set; } = null!;

    public string LastName { get; private set; } = null!;

    /// <summary>Küçük harfe normalize; benzersiz.</summary>
    public string Email { get; private set; } = null!;

    public string? Phone { get; private set; }

    /// <summary>ISO 3166-1 alpha-2 (TR, GB, DE, CY…).</summary>
    public string Nationality { get; private set; } = null!;

    public DateOnly DateOfBirth { get; private set; }

    public string LicenseNumber { get; private set; } = null!;

    public DateOnly LicenseIssuedOn { get; private set; }

    public string LicenseCountry { get; private set; } = null!;

    /// <summary>Simülasyonun ürettiği sahte müşteri (raporlarda ayrılabilsin).</summary>
    public bool IsSimulated { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public static Customer Register(
        string firstName, string lastName, string email, string? phone, string nationality,
        DateOnly dateOfBirth, string licenseNumber, DateOnly licenseIssuedOn, string licenseCountry,
        bool isSimulated, DateTimeOffset now)
    {
        var customer = new Customer
        {
            Id = Guid.CreateVersion7(),
            Email = NormalizeEmail(email),
            IsSimulated = isSimulated,
            CreatedAt = now,
        };
        if (!customer.Email.Contains('@') || customer.Email.StartsWith('@') || customer.Email.EndsWith('@'))
        {
            throw new DomainException("customer.invalid_email", "Geçerli bir e-posta adresi girin.");
        }

        customer.Update(firstName, lastName, phone, nationality, dateOfBirth, licenseNumber, licenseIssuedOn, licenseCountry, now);
        return customer;
    }

    public void Update(
        string firstName, string lastName, string? phone, string nationality,
        DateOnly dateOfBirth, string licenseNumber, DateOnly licenseIssuedOn, string licenseCountry, DateTimeOffset now)
    {
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            throw new DomainException("customer.name_required", "Ad ve soyad zorunlu.");
        }

        if (dateOfBirth.AddYears(18) > today)
        {
            throw new DomainException("customer.underage", "Müşteri 18 yaşından büyük olmalı.");
        }

        if (licenseIssuedOn > today || licenseIssuedOn < dateOfBirth.AddYears(16))
        {
            throw new DomainException("customer.invalid_license_date", "Ehliyet veriliş tarihi geçersiz.");
        }

        if (nationality.Length != 2 || licenseCountry.Length != 2)
        {
            throw new DomainException("customer.invalid_country", "Uyruk ve ehliyet ülkesi ISO-3166 iki harfli kod olmalı.");
        }

        if (string.IsNullOrWhiteSpace(licenseNumber))
        {
            throw new DomainException("customer.license_required", "Ehliyet numarası zorunlu.");
        }

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Phone = phone?.Trim();
        Nationality = nationality.ToUpperInvariant();
        DateOfBirth = dateOfBirth;
        LicenseNumber = licenseNumber.Trim();
        LicenseIssuedOn = licenseIssuedOn;
        LicenseCountry = licenseCountry.ToUpperInvariant();
    }
}
