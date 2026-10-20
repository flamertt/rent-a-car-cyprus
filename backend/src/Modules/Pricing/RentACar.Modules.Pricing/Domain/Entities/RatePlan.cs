using RentACar.Shared.Abstractions.Exceptions;

namespace RentACar.Modules.Pricing.Domain.Entities;

/// <summary>Sınıf başına düşük sezon günlük taban fiyatı (TRY). Sezon çarpanı üstüne uygulanır.</summary>
internal sealed class RatePlan
{
    private RatePlan()
    {
    }

    public Guid Id { get; private set; }

    public Guid CategoryId { get; private set; }

    /// <summary>Okunabilirlik için kopya (Fleet modülünün kodu; yabancı anahtar yok).</summary>
    public string CategoryCode { get; private set; } = null!;

    public decimal DailyRate { get; private set; }

    public static RatePlan Create(Guid categoryId, string categoryCode, decimal dailyRate)
    {
        var plan = new RatePlan { Id = Guid.CreateVersion7(), CategoryId = categoryId, CategoryCode = categoryCode };
        plan.SetDailyRate(dailyRate);
        return plan;
    }

    public void SetDailyRate(decimal dailyRate)
    {
        if (dailyRate <= 0)
        {
            throw new DomainException("pricing.invalid_rate", "Günlük fiyat sıfırdan büyük olmalı.");
        }

        DailyRate = dailyRate;
    }
}
