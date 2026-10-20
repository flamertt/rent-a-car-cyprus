namespace RentACar.Modules.Reservations.Domain.Rules;

/// <summary>İş kuralı parametreleri (appsettings: <c>Reservations</c> bölümü).</summary>
internal sealed class ReservationRules
{
    public const string SectionName = "Reservations";

    /// <summary>İadeden sonra temizlik/kontrol için araç kapalı kalır.</summary>
    public int TurnaroundMinutes { get; set; } = 120;

    /// <summary>Rezervasyon en az bu kadar önce yapılmalı.</summary>
    public int MinLeadMinutes { get; set; } = 60;

    public int MaxAdvanceDays { get; set; } = 365;

    public int MinLicenseYears { get; set; } = 1;

    /// <summary>Teslim saatinden bu kadar sonra gelmeyen rezervasyon "gelmedi" sayılır.</summary>
    public int NoShowAfterMinutes { get; set; } = 360;

    /// <summary>Teslim saatinden bu kadar önce araç verilebilir.</summary>
    public int EarlyPickupMinutes { get; set; } = 120;

    /// <summary>
    /// Eksik yakıt bedeli: tam depo karşılığı (TRY), eksik oran kadar yansıtılır.
    /// ≈ 45 lt × 77,12 TL (95 oktan, 17.09.2026 tavan fiyatı) + hizmet bedeli.
    /// </summary>
    public decimal RefuelPricePerTank { get; set; } = 4_000;

    public int GraceMinutes { get; set; } = 59;

    public TimeSpan Turnaround => TimeSpan.FromMinutes(TurnaroundMinutes);
}
