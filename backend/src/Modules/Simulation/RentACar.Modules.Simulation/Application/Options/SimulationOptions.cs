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

    /// <summary>Yetişme hızı (simüle saniye ÷ gerçek saniye).</summary>
    public double CatchUpSpeed { get; set; } = 3600;

    /// <summary>İlk açılışta bu kadar gün geriden başlanır; panel boş değil, geçmişi olan bir operasyonla açılır.</summary>
    public int WarmUpDays { get; set; } = 7;

    /// <summary>
    /// Haritada her an en az bu kadar araç yolda olsun (güçlü filo varsayımı; 0 = kapalı). Eksik kalırsa park hâlindeki
    /// müşteriler erken yola çıkar, yetmezse boştaki araçlar şubeler arasında transfer edilir (bkz. ActivityFloor).
    /// </summary>
    public int MinMovingVehicles { get; set; } = 50;

    public RoutingOptions Routing { get; set; } = new();
}
