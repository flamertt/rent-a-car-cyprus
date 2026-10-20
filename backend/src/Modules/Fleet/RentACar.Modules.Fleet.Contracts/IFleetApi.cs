using RentACar.Modules.Fleet.Contracts.Dtos;

namespace RentACar.Modules.Fleet.Contracts;

/// <summary>Filo modülünün diğer modüllere açtığı tek yüz.</summary>
public interface IFleetApi
{
    Task<IReadOnlyList<VehicleCategoryDto>> ListCategoriesAsync(CancellationToken cancellationToken = default);

    Task<VehicleCategoryDto?> GetCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);

    Task<VehicleDto?> GetVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiralanabilir (bakımda/emekli olmayan) araçlar, az kilometreli önce; <paramref name="categoryId"/> boşsa tüm sınıflar.
    /// Takvim çakışması çağıranın (Reservations) işidir.
    /// </summary>
    Task<IReadOnlyList<RentableVehicleDto>> ListRentableVehiclesAsync(Guid? categoryId = null, CancellationToken cancellationToken = default);

    Task ReportTelemetryAsync(IReadOnlyList<VehicleTelemetryDto> telemetry, CancellationToken cancellationToken = default);

    /// <summary>Teslim personeli aracı yola çıkardı (InTransit) / şubeye bıraktı (Available).</summary>
    Task SetInTransitAsync(Guid vehicleId, bool inTransit, Guid? arrivedBranchId, CancellationToken cancellationToken = default);
}
