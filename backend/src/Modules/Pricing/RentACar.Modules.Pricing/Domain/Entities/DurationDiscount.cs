using RentACar.Shared.Abstractions.Exceptions;

namespace RentACar.Modules.Pricing.Domain.Entities;

internal sealed class DurationDiscount
{
    private DurationDiscount()
    {
    }

    public Guid Id { get; private set; }

    public int MinDays { get; private set; }

    /// <summary>Kiralama bedelinden indirim yüzdesi (0–60).</summary>
    public decimal Percent { get; private set; }

    public static DurationDiscount Create(int minDays, decimal percent)
    {
        if (minDays < 2 || percent is <= 0 or > 60)
        {
            throw new DomainException("pricing.invalid_discount", "İndirim en az 2 gün ve %0–60 olmalı.");
        }

        return new DurationDiscount { Id = Guid.CreateVersion7(), MinDays = minDays, Percent = percent };
    }
}
