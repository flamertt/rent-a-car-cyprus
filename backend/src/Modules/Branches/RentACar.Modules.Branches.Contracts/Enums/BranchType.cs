namespace RentACar.Modules.Branches.Contracts.Enums;

public enum BranchType
{
    /// <summary>Personelli ofis: araç park alanı, teslim/iade, bakım.</summary>
    Office,

    /// <summary>Havalimanı karşılama masası (Ercan).</summary>
    AirportDesk,

    /// <summary>Liman karşılama noktası (Girne Turizm Limanı, Gazimağusa).</summary>
    PortDesk,

    /// <summary>Yeşil Hat geçiş noktasında buluşma (güneydeki havalimanlarından gelenler için).</summary>
    BorderCrossing,

    /// <summary>Otel / adres teslim noktası (personel yok, araç getirilir).</summary>
    DeliveryPoint,
}
