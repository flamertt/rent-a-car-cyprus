using RentACar.Modules.Fleet.Contracts.Enums;
using RentACar.Shared.Abstractions.Exceptions;

namespace RentACar.Modules.Fleet.Domain.Entities;

/// <summary>
/// Kiralama sınıfı (ACRISS kodu, örn. ECAR = Ekonomi · 4 kapı · otomatik · klimalı benzinli).
/// Müşteri sınıf kiralar, plaka değil: araç ataması rezervasyonda yapılır.
/// </summary>
internal sealed class VehicleCategory
{
    private VehicleCategory()
    {
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string ExampleModels { get; private set; } = null!;

    public BodyType Body { get; private set; }

    public Transmission Transmission { get; private set; }

    public FuelType Fuel { get; private set; }

    public int Seats { get; private set; }

    public int Doors { get; private set; }

    public int Bags { get; private set; }

    public int MinDriverAge { get; private set; }

    /// <summary>Kredi kartından bloke edilen depozito (TRY).</summary>
    public decimal Deposit { get; private set; }

    public int SortOrder { get; private set; }

    public static VehicleCategory Create(
        string code, string name, string exampleModels, BodyType body, Transmission transmission, FuelType fuel,
        int seats, int doors, int bags, int minDriverAge, decimal deposit, int sortOrder)
    {
        if (code.Length != 4)
        {
            throw new DomainException("category.invalid_code", "ACRISS kodu 4 karakter olmalı.");
        }

        if (minDriverAge is < 18 or > 30)
        {
            throw new DomainException("category.invalid_age", "Asgari sürücü yaşı 18–30 arasında olmalı.");
        }

        return new VehicleCategory
        {
            Id = Guid.CreateVersion7(),
            Code = code.ToUpperInvariant(),
            Name = name,
            ExampleModels = exampleModels,
            Body = body,
            Transmission = transmission,
            Fuel = fuel,
            Seats = seats,
            Doors = doors,
            Bags = bags,
            MinDriverAge = minDriverAge,
            Deposit = deposit,
            SortOrder = sortOrder,
        };
    }
}
