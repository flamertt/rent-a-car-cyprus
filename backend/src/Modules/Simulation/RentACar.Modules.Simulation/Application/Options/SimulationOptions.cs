namespace RentACar.Modules.Simulation.Application.Options;

/// <summary>appsettings: <c>Simulation</c> bölümü.</summary>
internal sealed class SimulationOptions
{
    public const string SectionName = "Simulation";

    /// <summary>İlk açılışta simülasyon çalışsın mı (sonrası veritabanındaki durumdan).</summary>
    public bool AutoStart { get; set; } = true;

    /// <summary>Motor döngüsü (gerçek zaman).</summary>
    public int TickMilliseconds { get; set; } = 500;

    /// <summary>Mevsimsel talebin çarpanı (panelde daha canlı bir operasyon için 1'in üstü).</summary>
    public double DemandMultiplier { get; set; } = 1.5;

    /// <summary>
    /// Saat gerçek saate kilitli (1×): panel gerçek zamanlı bir operasyon gibi görünür. Saat geride kalırsa
    /// (ilk açılış, sunucunun kapalı kaldığı süre) <see cref="CatchUpSpeed"/> ile hızlı ileri sarılıp yetişir.
    /// </summary>
    public bool FollowRealTime { get; set; } = true;

    /// <summary>
    /// Yetişme hızı: her karede saat <see cref="TickMilliseconds"/> × bu kadar ilerler (500 ms × 3600 = 30 dk). Kare uzun
    /// sürerse yetişme yavaşlar; adım büyümez, simüle zaman atlanmaz.
    /// </summary>
    public double CatchUpSpeed { get; set; } = 3600;

    /// <summary>İlk açılışta bu kadar gün geriden başlanır; panel boş değil, geçmişi olan bir operasyonla açılır.</summary>
    public int WarmUpDays { get; set; } = 7;

    /// <summary>
    /// Haritada her an en az bu kadar araç yolda olsun (güçlü filo varsayımı; 0 = kapalı). Eksik kalırsa yakında çıkacak
    /// müşteriler biraz erken yola çıkar, yetmezse boştaki araçlar servis turuna çıkar (bkz. ActivityFloor).
    /// </summary>
    public int MinMovingVehicles { get; set; } = 50;

    /// <summary>
    /// Güçlü filo: kirada araç oranı hedefi. Düzenli talep tek başına bunu aşmaz; altında kalınırsa kısa vadeli (aynı gün)
    /// talep eklenir; yoldaki araçların çoğu gerçek müşterilerden gelsin (0 = kapalı).
    /// </summary>
    public double TargetUtilization { get; set; } = 0.75;

    /// <summary>Tavan: 24 saat içindeki teslimlerde kirada araç oranı bunu geçmesin (0 = kapalı).</summary>
    public double MaxUtilization { get; set; } = 0.8;

    public RoutingOptions Routing { get; set; } = new();
}
