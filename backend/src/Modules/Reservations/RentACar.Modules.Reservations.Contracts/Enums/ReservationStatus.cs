namespace RentACar.Modules.Reservations.Contracts.Enums;

public enum ReservationStatus
{
    /// <summary>Araç atanmış, teslim bekleniyor.</summary>
    Confirmed,

    /// <summary>Araç müşteride.</summary>
    Active,

    Completed,

    Cancelled,

    NoShow,
}
