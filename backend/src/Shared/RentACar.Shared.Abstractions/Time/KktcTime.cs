namespace RentACar.Shared.Abstractions.Time;

/// <summary>
/// KKTC yerel saati. Kuzey 2016–2017 arasında sürekli UTC+3 kullandı, 2017'den beri adanın geri kalanı gibi
/// EET/EEST (UTC+2/+3, yaz saati) uyguluyor; IANA'da Asia/Famagusta ile Europe/Nicosia bugün aynı kuralda.
/// Veritabanında her zaman UTC tutulur; çalışma saati gibi yerel kurallar bu dönüşümle kontrol edilir.
/// </summary>
public static class KktcTime
{
    public static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Nicosia");

    public static DateTimeOffset ToLocal(DateTimeOffset utc) => TimeZoneInfo.ConvertTime(utc, Zone);

    public static TimeOnly LocalTimeOfDay(DateTimeOffset utc) => TimeOnly.FromDateTime(ToLocal(utc).DateTime);

    public static DateOnly LocalDate(DateTimeOffset utc) => DateOnly.FromDateTime(ToLocal(utc).DateTime);
}
