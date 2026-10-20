using RentACar.Modules.Pricing.Contracts.Enums;
using RentACar.Modules.Pricing.Domain.Entities;

namespace RentACar.Modules.Pricing.Infrastructure.Persistence.Seed;

/// <summary>
/// Başlangıç tarifesi (TRY, düşük sezon günlük). Değerler simülasyon içindir; admin panelinden düzenlenir.
/// Bant: Haziran 2026 piyasasında ekonomi 1.400–2.800 TL/gün (docs/KKTC-VERI-KAYNAKLARI.md §10); yüksek
/// sezon çarpanıyla (1,7) ekonomi ≈ 2.465 TL. Kuzeyde kiralanan araç güneye geçirilemez (sözleşme +
/// KKTC sigortası güneyde geçersiz) → "güneye geçiş" ekstrası bilerek yok.
/// Sezonlar KKTC turizm takvimine göre: yaz (15 Haz – 15 Eyl) zirve, bahar/sonbahar ara, kış düşük;
/// Noel–Yılbaşı (İngiliz ve Kuzey Avrupa ziyaretçileri) ayrı tepe.
/// </summary>
internal static class PricingSeedData
{
    public const decimal DefaultDailyRate = 2_000;

    public static readonly IReadOnlyDictionary<string, decimal> BaseRates = new Dictionary<string, decimal>
    {
        ["MCAR"] = 1_250, ["ECAR"] = 1_450, ["CDAR"] = 1_800, ["CDAH"] = 1_950, ["CFAR"] = 2_600,
        ["IFAR"] = 3_200, ["IVAR"] = 3_600, ["PDAR"] = 5_200, ["STAR"] = 4_800,
    };

    public static IEnumerable<Season> Seasons()
    {
        yield return Season.Create("Ara sezon (bahar)", 4, 1, 6, 14, 1.30m);
        yield return Season.Create("Yüksek sezon", 6, 15, 9, 15, 1.70m);
        yield return Season.Create("Ara sezon (sonbahar)", 9, 16, 10, 31, 1.30m);
        yield return Season.Create("Noel – Yılbaşı", 12, 20, 1, 3, 1.40m);
    }

    public static IEnumerable<DurationDiscount> Discounts()
    {
        yield return DurationDiscount.Create(3, 5);
        yield return DurationDiscount.Create(7, 12);
        yield return DurationDiscount.Create(14, 20);
        yield return DurationDiscount.Create(28, 30);
    }

    public static IEnumerable<Extra> Extras()
    {
        yield return Extra.Create("BABY_SEAT", "Bebek koltuğu (0–4 yaş)", "ECE R44/R129 onaylı", ExtraUnit.PerDay, 150, 3, 10);
        yield return Extra.Create("BOOSTER", "Yükseltici koltuk (4–12 yaş)", "", ExtraUnit.PerDay, 100, 3, 10);
        yield return Extra.Create("EXTRA_DRIVER", "Ek sürücü", "Ek sürücü de ehliyet ve yaş şartını sağlamalı", ExtraUnit.PerDay, 200, 2, 10);
        yield return Extra.Create("FULL_COVER", "Tam koruma", "Hasar muafiyetini sıfırlar", ExtraUnit.PerDay, 450, 1, null);
        yield return Extra.Create("TYRE_GLASS", "Lastik · cam · far güvencesi", "", ExtraUnit.PerDay, 120, 1, null);
        yield return Extra.Create("WIFI", "Mobil Wi-Fi cihazı", "", ExtraUnit.PerDay, 120, 1, null);
        yield return Extra.Create("AIRPORT_MEET", "Karşılama tabelası", "Ercan çıkışında isimle karşılama", ExtraUnit.PerRental, 250, 1, null);
    }
}
