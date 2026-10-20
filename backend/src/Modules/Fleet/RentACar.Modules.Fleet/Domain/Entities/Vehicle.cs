using NetTopologySuite.Geometries;
using RentACar.Modules.Fleet.Contracts.Enums;
using RentACar.Modules.Fleet.Domain.Services;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Geo;

namespace RentACar.Modules.Fleet.Domain.Entities;

internal sealed class Vehicle
{
    private Vehicle()
    {
    }

    public Guid Id { get; private set; }

    public string Plate { get; private set; } = null!;

    public string Make { get; private set; } = null!;

    public string Model { get; private set; } = null!;

    public int Year { get; private set; }

    public string Color { get; private set; } = null!;

    public Guid CategoryId { get; private set; }

    public VehicleCategory Category { get; private set; } = null!;

    public VehicleStatus Status { get; private set; }

    /// <summary>Aracın bulunduğu şube; yoldayken (kirada / transferde) null.</summary>
    public Guid? BranchId { get; private set; }

    public Point Position { get; private set; } = null!;

    public double HeadingDeg { get; private set; }

    public double SpeedKmh { get; private set; }

    public int OdometerKm { get; private set; }

    /// <summary>Depo doluluğu 0..1.</summary>
    public double FuelLevel { get; private set; }

    public DateTimeOffset? LastSeenAt { get; private set; }

    /// <summary>Bulunduğu şubenin otoparkındaki yer numarası; her park edişte o şubenin ilk boş yeri (bkz. <see cref="ParkingLot"/>).</summary>
    public int ParkingSlot { get; private set; }

    public static string NormalizePlate(string plate) => plate.Trim().ToUpperInvariant();

    public static Vehicle Create(
        string plate, string make, string model, int year, string color,
        VehicleCategory category, Guid branchId, GeoPoint branchLocation, int parkingSlot, int odometerKm)
    {
        if (string.IsNullOrWhiteSpace(plate))
        {
            throw new DomainException("vehicle.plate_required", "Plaka zorunlu.");
        }

        return new Vehicle
        {
            Id = Guid.CreateVersion7(),
            Plate = NormalizePlate(plate),
            Make = make,
            Model = model,
            Year = year,
            Color = color,
            CategoryId = category.Id,
            Category = category,
            Status = VehicleStatus.Available,
            BranchId = branchId,
            ParkingSlot = parkingSlot,
            Position = ParkingLot.SpotFor(branchLocation, parkingSlot).ToPoint(),
            OdometerKm = odometerKm,
            FuelLevel = 1,
        };
    }

    public void StartRental(int odometerKm, double fuelLevel, DateTimeOffset at)
    {
        if (Status is not (VehicleStatus.Available or VehicleStatus.InTransit))
        {
            throw new ConflictException("vehicle.not_rentable", $"{Plate} şu an teslim edilemez ({Status}).");
        }

        Status = VehicleStatus.OnRent;
        BranchId = null;
        SetMeters(odometerKm, fuelLevel);
        LastSeenAt = at;
    }

    public void EndRental(Guid branchId, GeoPoint branchLocation, int parkingSlot, int odometerKm, double fuelLevel, DateTimeOffset at)
    {
        if (Status != VehicleStatus.OnRent)
        {
            throw new ConflictException("vehicle.not_on_rent", $"{Plate} kirada değil ({Status}).");
        }

        Park(branchId, branchLocation, parkingSlot, at);
        SetMeters(odometerKm, fuelLevel);
    }

    public void SetInTransit(DateTimeOffset at)
    {
        if (Status != VehicleStatus.Available)
        {
            throw new ConflictException("vehicle.not_available", $"{Plate} transfere çıkamaz ({Status}).");
        }

        Status = VehicleStatus.InTransit;
        BranchId = null;
        LastSeenAt = at;
    }

    public void Arrive(Guid branchId, GeoPoint branchLocation, int parkingSlot, DateTimeOffset at)
    {
        if (Status != VehicleStatus.InTransit)
        {
            throw new ConflictException("vehicle.not_in_transit", $"{Plate} transferde değil ({Status}).");
        }

        Park(branchId, branchLocation, parkingSlot, at);
    }

    /// <summary>Yönetim: bakıma al / bakımdan çıkar / filodan çıkar.</summary>
    public void ChangeStatus(VehicleStatus target)
    {
        var allowed = (Status, target) switch
        {
            (VehicleStatus.Available, VehicleStatus.Maintenance) => true,
            (VehicleStatus.Maintenance, VehicleStatus.Available) => true,
            (VehicleStatus.Available or VehicleStatus.Maintenance, VehicleStatus.Retired) => true,
            _ => false,
        };
        if (!allowed)
        {
            throw new ConflictException("vehicle.invalid_transition", $"{Plate}: {Status} → {target} geçişine izin yok.");
        }

        Status = target;
    }

    private void Park(Guid branchId, GeoPoint branchLocation, int parkingSlot, DateTimeOffset at)
    {
        Status = VehicleStatus.Available;
        BranchId = branchId;
        ParkingSlot = parkingSlot;
        Position = ParkingLot.SpotFor(branchLocation, parkingSlot).ToPoint();
        SpeedKmh = 0;
        LastSeenAt = at;
    }

    private void SetMeters(int odometerKm, double fuelLevel)
    {
        if (odometerKm < OdometerKm)
        {
            throw new DomainException("vehicle.odometer_rollback", $"Kilometre geri alınamaz ({OdometerKm} → {odometerKm}).");
        }

        OdometerKm = odometerKm;
        FuelLevel = Math.Clamp(fuelLevel, 0, 1);
    }
}
