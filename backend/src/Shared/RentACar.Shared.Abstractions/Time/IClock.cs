namespace RentACar.Shared.Abstractions.Time;

/// <summary>
/// Uygulama saati. Simülasyonda gerçek zamandan hızlı akar (örn. 1 gerçek dakika = 1 saat);
/// iş kuralları (rezervasyon başlangıcı, gecikme, sezon) DateTimeOffset.UtcNow yerine bunu okur.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }

    /// <summary>Simülasyon hızı (gerçek saniye başına simüle saniye). 0 = duraklatılmış.</summary>
    double Speed { get; }

    void SetSpeed(double speed);

    /// <summary>Saati verilen ana taşır (yeniden başlatmada kaldığı yerden devam için).</summary>
    void Reset(DateTimeOffset utcNow);
}
