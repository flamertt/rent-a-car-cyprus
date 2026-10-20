namespace RentACar.Modules.Fleet.Contracts.Enums;

/// <summary>Aracın fiziksel durumu. Takvim (gelecek rezervasyonlar) Reservations modülündedir.</summary>
public enum VehicleStatus
{
    /// <summary>Bir şubede, kiralamaya hazır.</summary>
    Available,

    /// <summary>Müşteride.</summary>
    OnRent,

    /// <summary>Personel tarafından teslim noktasına / başka şubeye götürülüyor.</summary>
    InTransit,

    Maintenance,

    Retired,
}
