using RentACar.Shared.Abstractions.Geo;

namespace RentACar.Modules.Branches.Contracts;

/// <summary>KKTC'nin altı ilçesi (idari bölge).</summary>
public enum District
{
    Lefkosa,
    Girne,
    Gazimagusa,
    Guzelyurt,
    Iskele,
    Lefke,
}

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

public sealed record BranchDto(
    Guid Id,
    string Code,
    string Name,
    BranchType Type,
    District District,
    string Address,
    GeoPoint Location,
    string? Phone,
    TimeOnly OpensAt,
    TimeOnly ClosesAt,
    bool IsActive,
    decimal DeliveryFee);

/// <summary>Şubeler modülünün diğer modüllere açtığı tek yüz.</summary>
public interface IBranchesApi
{
    Task<BranchDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BranchDto>> ListActiveAsync(CancellationToken cancellationToken = default);
}
