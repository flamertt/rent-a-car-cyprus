using System.Text.Json.Serialization;

namespace RentACar.Shared.Abstractions.Geo;

/// <summary>WGS84 nokta. Sözleşmelerde ve domain'de kullanılan değer tipi.</summary>
public readonly record struct GeoPoint(double Lon, double Lat)
{
    /// <summary>KKTC'yi kabaca kapsayan kutu (Yeşil Hat'ın kuzeyi + Karpaz).</summary>
    public static readonly (double MinLon, double MinLat, double MaxLon, double MaxLat) KktcBounds =
        (32.60, 35.00, 34.65, 35.75);

    /// <summary>
    /// Kaba kutu kontrolü (güney Lefkoşa'yı da kapsar). Kesin kontrol için KKTC sınır poligonu
    /// PostGIS'e alınıp ST_Within ile yapılmalı (yol haritası).
    /// </summary>
    [JsonIgnore]
    public bool IsInsideKktcBounds =>
        Lon >= KktcBounds.MinLon && Lon <= KktcBounds.MaxLon && Lat >= KktcBounds.MinLat && Lat <= KktcBounds.MaxLat;
}
