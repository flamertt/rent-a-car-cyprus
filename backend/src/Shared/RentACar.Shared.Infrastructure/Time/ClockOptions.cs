namespace RentACar.Shared.Infrastructure.Time;

public sealed class ClockOptions
{
    public const string SectionName = "Clock";

    /// <summary>Boşsa gerçek "şimdi"den başlar. Simülasyon modülü kaydedilmiş anı varsa onu geri yükler.</summary>
    public DateTimeOffset? StartAt { get; set; }

    /// <summary>Gerçek saniye başına simüle saniye (1 = gerçek zaman, 60 = 1 dk'da 1 saat).</summary>
    public double Speed { get; set; } = 1;
}
