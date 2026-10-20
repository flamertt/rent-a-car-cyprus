namespace RentACar.Modules.Pricing.Domain.Entities;

/// <summary>Şirket geneli ücret kuralları (tek satır).</summary>
internal sealed class PricingPolicy
{
    public static readonly Guid SingletonId = new("00000000-0000-0000-0000-000000000001");

    public Guid Id { get; private set; } = SingletonId;

    /// <summary>Farklı ilçede iade.</summary>
    public decimal OneWayFee { get; private set; } = 750;

    /// <summary>Şube kapalıyken teslim/iade (her biri için).</summary>
    public decimal OutOfHoursFee { get; private set; } = 500;

    /// <summary>Bu yaş ve altı genç sürücü sayılır.</summary>
    public int YoungDriverMaxAge { get; private set; } = 24;

    public decimal YoungDriverDailyFee { get; private set; } = 250;

    /// <summary>İadede ek gün yazılmadan önce tanınan tolerans.</summary>
    public int GraceMinutes { get; private set; } = 59;

    public void Update(decimal oneWayFee, decimal outOfHoursFee, int youngDriverMaxAge, decimal youngDriverDailyFee, int graceMinutes)
    {
        OneWayFee = Math.Max(0, oneWayFee);
        OutOfHoursFee = Math.Max(0, outOfHoursFee);
        YoungDriverMaxAge = Math.Clamp(youngDriverMaxAge, 18, 30);
        YoungDriverDailyFee = Math.Max(0, youngDriverDailyFee);
        GraceMinutes = Math.Clamp(graceMinutes, 0, 180);
    }
}
