namespace RentACar.Modules.Fleet.Contracts.Dtos;

/// <summary>Atama adayı: rezervasyon modülü takvim çakışmasını kendisi eler.</summary>
public sealed record RentableVehicleDto(Guid Id, Guid CategoryId, string Plate, Guid? BranchId, int OdometerKm);
