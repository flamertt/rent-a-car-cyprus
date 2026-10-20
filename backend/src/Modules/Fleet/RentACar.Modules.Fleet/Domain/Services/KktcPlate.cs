namespace RentACar.Modules.Fleet.Domain.Services;

/// <summary>
/// Kiralık araç ("Z" izinli) plakası: <c>Z</c> + iki harf + boşluk + üç rakam, örn. "ZAK 482"; kırmızı zemin,
/// siyah yazı (Wikipedia "Vehicle registration plates of Cyprus", Cyprus Mail 2024-07-25; bkz.
/// docs/KKTC-VERI-KAYNAKLARI.md §5.5 — bir kaynak beyaz zemin + kırmızı Z diyor). "ZT" geçici, "ZZ" ziyaretçi
/// plakası olduğu için ikinci harf T/Z olmaz. Rastgele üretilir; gerçek bir plakayla eşleşmesi amaçlanmaz.
/// </summary>
internal static class KktcPlate
{
    // Türk alfabesine özgü harfler (Ç, Ğ, İ, Ö, Ş, Ü) ve karışabilen I/O/Q kullanılmaz.
    private const string Letters = "ABCDEFGHJKLMNPRSTUVYZ";
    private const string SecondLetters = "ABCDEFGHJKLMNPRSUVY";

    public static string RentalPlate(Random random) =>
        $"Z{SecondLetters[random.Next(SecondLetters.Length)]}{Letters[random.Next(Letters.Length)]} {random.Next(1, 1000):000}";
}
