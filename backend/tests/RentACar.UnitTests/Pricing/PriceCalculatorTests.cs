using RentACar.Modules.Pricing.Contracts.Dtos;
using RentACar.Modules.Pricing.Contracts.Enums;
using RentACar.Modules.Pricing.Contracts.Requests;
using RentACar.Modules.Pricing.Domain.Entities;
using RentACar.Modules.Pricing.Domain.Services;
using RentACar.Modules.Pricing.Domain.ValueObjects;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.UnitTests.Pricing;

public sealed class PriceCalculatorTests
{
    private static readonly BranchPricingInfo GirneOffice = new("Girne Merkez Ofis", "Girne", 0, new TimeOnly(8, 30), new TimeOnly(19, 0));
    private static readonly BranchPricingInfo GirnePort = new("Girne Turizm Limanı", "Girne", 350, new TimeOnly(7, 0), new TimeOnly(22, 0));
    private static readonly BranchPricingInfo Ercan = new("Ercan Havalimanı Karşılama", "Lefkosa", 0, new TimeOnly(0, 0), new TimeOnly(23, 59));

    [Theory]
    [InlineData(24 * 60, 1)]
    [InlineData(24 * 60 + 30, 1)] // tolerans (59 dk) içinde
    [InlineData(24 * 60 + 61, 2)]
    [InlineData(5 * 24 * 60, 5)]
    [InlineData(90, 1)]
    public void RentalDays_counts_24h_blocks_with_grace(int minutes, int expected)
    {
        var pickup = new DateTimeOffset(2026, 11, 3, 10, 0, 0, TimeSpan.Zero);
        Assert.Equal(expected, PriceCalculator.RentalDays(pickup, pickup.AddMinutes(minutes), 59));
    }

    [Fact]
    public void RentalDays_rejects_return_before_pickup()
    {
        var pickup = new DateTimeOffset(2026, 11, 3, 10, 0, 0, TimeSpan.Zero);
        var ex = Assert.Throws<DomainException>(() => PriceCalculator.RentalDays(pickup, pickup.AddHours(-1), 59));
        Assert.Equal("pricing.invalid_period", ex.Code);
    }

    [Fact]
    public void Each_day_uses_its_own_season_multiplier()
    {
        // 13–14 Haziran ara sezon (×1,3), 15–16 Haziran yüksek sezon (×1,7).
        var pickup = Local(2026, 6, 13, 10);
        var quote = Quote(pickup, pickup.AddDays(4), GirneOffice, GirneOffice, baseRate: 1_000);

        var rental = quote.Lines.Single(l => l.Code == "RENTAL");
        Assert.Equal(1_300 + 1_300 + 1_700 + 1_700, rental.Amount);
    }

    [Fact]
    public void Overlapping_seasons_take_the_highest_multiplier()
    {
        Season[] seasons = [Season.Create("Kış", 11, 1, 3, 31, 1.0m), Season.Create("Noel", 12, 20, 1, 3, 1.4m)];
        Assert.Equal(1.4m, PriceCalculator.SeasonMultiplier(seasons, new DateOnly(2026, 12, 31)));
        Assert.Equal(1.4m, PriceCalculator.SeasonMultiplier(seasons, new DateOnly(2027, 1, 2)));
        Assert.Equal(1.0m, PriceCalculator.SeasonMultiplier(seasons, new DateOnly(2027, 1, 10)));
        Assert.Equal(1m, PriceCalculator.SeasonMultiplier(seasons, new DateOnly(2026, 7, 1)));
    }

    [Fact]
    public void Largest_qualifying_duration_discount_applies()
    {
        var pickup = Local(2026, 11, 10, 10);
        var quote = Quote(pickup, pickup.AddDays(8), GirneOffice, GirneOffice, baseRate: 1_000);

        var discount = quote.Lines.Single(l => l.Code == "DISCOUNT");
        Assert.Equal(-960, discount.Amount); // 8 gün × 1.000 × %12 (7+ kademe; 3+ değil)
        Assert.Equal(8_000 - 960, quote.Total);
    }

    [Fact]
    public void Fees_for_young_driver_delivery_point_and_one_way()
    {
        var pickup = Local(2026, 11, 10, 10);
        var quote = Quote(pickup, pickup.AddDays(2), GirnePort, Ercan, baseRate: 1_000, driverAge: 22);

        Assert.Equal(500, quote.Lines.Single(l => l.Code == "YOUNG_DRIVER").Amount); // 2 gün × 250
        Assert.Equal(350, quote.Lines.Single(l => l.Code == "DELIVERY_PICKUP").Amount);
        Assert.Equal(750, quote.Lines.Single(l => l.Code == "ONE_WAY").Amount); // Girne → Lefkoşa ilçesi
        Assert.DoesNotContain(quote.Lines, l => l.Code == "DELIVERY_RETURN");
    }

    [Fact]
    public void Same_district_return_is_not_one_way()
    {
        var pickup = Local(2026, 11, 10, 10);
        var quote = Quote(pickup, pickup.AddDays(2), GirneOffice, GirnePort, baseRate: 1_000);

        Assert.DoesNotContain(quote.Lines, l => l.Code == "ONE_WAY");
        Assert.Contains(quote.Lines, l => l.Code == "DELIVERY_RETURN");
    }

    [Fact]
    public void Out_of_hours_pickup_is_charged_in_local_time()
    {
        var pickup = Local(2026, 11, 10, 7); // 07:00 yerel, ofis 08:30'da açılır
        var quote = Quote(pickup, Local(2026, 11, 12, 10), GirneOffice, GirneOffice, baseRate: 1_000);

        Assert.Contains(quote.Lines, l => l.Code == "OUT_OF_HOURS_PICKUP");
        Assert.DoesNotContain(quote.Lines, l => l.Code == "OUT_OF_HOURS_RETURN");
    }

    [Fact]
    public void Daily_extra_is_capped_by_max_charge_days()
    {
        var pickup = Local(2026, 11, 10, 10);
        var quote = Quote(pickup, pickup.AddDays(14), GirneOffice, GirneOffice, baseRate: 1_000,
            extras: [new QuoteExtraRequest("baby_seat", 2)]);

        var line = quote.Lines.Single(l => l.Code == "BABY_SEAT");
        Assert.Equal(150 * 10 * 2, line.Amount);
    }

    [Fact]
    public void Unknown_or_excess_extra_is_rejected()
    {
        var pickup = Local(2026, 11, 10, 10);
        Assert.Equal("pricing.unknown_extra", Assert.Throws<DomainException>(() =>
            Quote(pickup, pickup.AddDays(2), GirneOffice, GirneOffice, 1_000, extras: [new QuoteExtraRequest("JETPACK", 1)])).Code);
        Assert.Equal("pricing.extra_quantity", Assert.Throws<DomainException>(() =>
            Quote(pickup, pickup.AddDays(2), GirneOffice, GirneOffice, 1_000, extras: [new QuoteExtraRequest("BABY_SEAT", 4)])).Code);
    }

    private static QuoteDto Quote(
        DateTimeOffset pickupAt, DateTimeOffset returnAt, BranchPricingInfo pickup, BranchPricingInfo ret, decimal baseRate,
        int? driverAge = 35, IReadOnlyList<QuoteExtraRequest>? extras = null)
    {
        var inputs = new PricingInputs(
            baseRate,
            5_000,
            [Season.Create("Bahar", 4, 1, 6, 14, 1.3m), Season.Create("Yaz", 6, 15, 9, 15, 1.7m)],
            [DurationDiscount.Create(3, 5), DurationDiscount.Create(7, 12), DurationDiscount.Create(14, 20)],
            new Dictionary<string, Extra>
            {
                ["BABY_SEAT"] = Extra.Create("BABY_SEAT", "Bebek koltuğu", "", ExtraUnit.PerDay, 150, 3, 10),
            },
            new PricingPolicy(),
            pickup,
            ret);
        return PriceCalculator.Calculate(
            new QuoteRequest(Guid.NewGuid(), pickupAt, returnAt, Guid.NewGuid(), Guid.NewGuid(), extras, driverAge), inputs);
    }

    private static DateTimeOffset Local(int y, int m, int d, int hour) =>
        new(TimeZoneInfo.ConvertTimeToUtc(new DateTime(y, m, d, hour, 0, 0, DateTimeKind.Unspecified), KktcTime.Zone), TimeSpan.Zero);
}
