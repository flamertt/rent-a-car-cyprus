using RentACar.Shared.Abstractions.Exceptions;

namespace RentACar.Modules.Pricing.Domain.Entities;

/// <summary>Yıllık tekrar eden sezon aralığı (ay/gün). Yılbaşını aşabilir (20 Ara – 3 Oca).</summary>
internal sealed class Season
{
    private Season()
    {
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public int StartMonth { get; private set; }

    public int StartDay { get; private set; }

    public int EndMonth { get; private set; }

    public int EndDay { get; private set; }

    public decimal Multiplier { get; private set; }

    public static Season Create(string name, int startMonth, int startDay, int endMonth, int endDay, decimal multiplier)
    {
        var season = new Season { Id = Guid.CreateVersion7() };
        season.Update(name, startMonth, startDay, endMonth, endDay, multiplier);
        return season;
    }

    public void Update(string name, int startMonth, int startDay, int endMonth, int endDay, decimal multiplier)
    {
        if (multiplier is <= 0 or > 5)
        {
            throw new DomainException("pricing.invalid_multiplier", "Sezon çarpanı 0–5 arasında olmalı.");
        }

        if (!IsValidMonthDay(startMonth, startDay) || !IsValidMonthDay(endMonth, endDay))
        {
            throw new DomainException("pricing.invalid_season", "Sezon başlangıç/bitiş ay-gün değeri geçersiz.");
        }

        Name = name;
        StartMonth = startMonth;
        StartDay = startDay;
        EndMonth = endMonth;
        EndDay = endDay;
        Multiplier = multiplier;
    }

    public bool Contains(DateOnly date)
    {
        var d = date.Month * 100 + date.Day;
        var start = StartMonth * 100 + StartDay;
        var end = EndMonth * 100 + EndDay;
        return start <= end ? d >= start && d <= end : d >= start || d <= end;
    }

    // 2024 artık yıl: 29 Şubat geçerli sayılır.
    private static bool IsValidMonthDay(int month, int day) =>
        month is >= 1 and <= 12 && day >= 1 && day <= DateTime.DaysInMonth(2024, month);
}
