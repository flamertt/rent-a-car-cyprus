using RentACar.Modules.Pricing.Contracts.Constants;
using RentACar.Modules.Pricing.Contracts.Dtos;
using RentACar.Modules.Pricing.Contracts.Enums;
using RentACar.Modules.Pricing.Contracts.Requests;
using RentACar.Modules.Pricing.Domain.Entities;
using RentACar.Modules.Pricing.Domain.ValueObjects;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Pricing.Domain.Services;

/// <summary>
/// Fiyat hesabı (saf fonksiyon, veritabanı yok). Her kiralama günü kendi tarihinin sezon çarpanıyla
/// fiyatlanır; çakışan sezonlarda en yüksek çarpan geçerlidir. Tutarlar tam liraya yuvarlanır.
/// </summary>
internal static class PriceCalculator
{
    public const int MaxRentalDays = 90;

    public static int RentalDays(DateTimeOffset pickupAt, DateTimeOffset returnAt, int graceMinutes)
    {
        if (returnAt <= pickupAt)
        {
            throw new DomainException("pricing.invalid_period", "İade zamanı teslimden sonra olmalı.");
        }

        var billable = returnAt - pickupAt - TimeSpan.FromMinutes(graceMinutes);
        var days = Math.Max(1, (int)Math.Ceiling(billable.TotalHours / 24));
        if (days > MaxRentalDays)
        {
            throw new DomainException("pricing.too_long", $"En fazla {MaxRentalDays} günlük kiralama yapılabilir.");
        }

        return days;
    }

    public static QuoteDto Calculate(QuoteRequest request, PricingInputs inputs)
    {
        var policy = inputs.Policy;
        var days = RentalDays(request.PickupAt, request.ReturnAt, policy.GraceMinutes);
        var lines = new List<QuoteLineDto>();

        // 1) Kiralama bedeli: gün gün sezon çarpanı.
        var firstDay = KktcTime.LocalDate(request.PickupAt);
        var rental = Enumerable.Range(0, days)
            .Select(i => Round(inputs.BaseDailyRate * SeasonMultiplier(inputs.Seasons, firstDay.AddDays(i))))
            .Sum();
        lines.Add(new QuoteLineDto("RENTAL", $"Kiralama · {days} gün", days, Round(rental / days), rental));

        // 2) Uzun süre indirimi: hak edilen en büyük kademe.
        var discount = inputs.Discounts.Where(d => days >= d.MinDays).MaxBy(d => d.MinDays);
        if (discount is not null)
        {
            var amount = -Round(rental * discount.Percent / 100);
            lines.Add(new QuoteLineDto("DISCOUNT", $"%{discount.Percent:0.#} uzun süre indirimi ({discount.MinDays}+ gün)", 1, amount, amount));
        }

        // 3) Genç sürücü.
        if (request.DriverAge is { } age && age <= policy.YoungDriverMaxAge)
        {
            lines.Add(Line("YOUNG_DRIVER", $"Genç sürücü farkı ({policy.YoungDriverMaxAge} yaş ve altı)", days, policy.YoungDriverDailyFee));
        }

        // 4) Ekstralar.
        foreach (var selection in request.Extras ?? [])
        {
            lines.Add(ExtraLine(selection, inputs.Extras, days));
        }

        // 5) Teslim noktası, tek yön ve mesai dışı ücretleri.
        var pickup = inputs.PickupBranch;
        var ret = inputs.ReturnBranch;
        if (pickup.DeliveryFee > 0)
        {
            lines.Add(Line("DELIVERY_PICKUP", $"Teslim noktası: {pickup.Name}", 1, pickup.DeliveryFee));
        }

        if (ret.DeliveryFee > 0)
        {
            lines.Add(Line("DELIVERY_RETURN", $"İade noktası: {ret.Name}", 1, ret.DeliveryFee));
        }

        if (pickup.District != ret.District)
        {
            lines.Add(Line("ONE_WAY", "Farklı ilçede iade (tek yön)", 1, policy.OneWayFee));
        }

        if (!pickup.IsOpenAt(KktcTime.LocalTimeOfDay(request.PickupAt)))
        {
            lines.Add(Line("OUT_OF_HOURS_PICKUP", $"Mesai dışı teslim ({pickup.OpensAt:HH\\:mm}–{pickup.ClosesAt:HH\\:mm} dışı)", 1, policy.OutOfHoursFee));
        }

        if (!ret.IsOpenAt(KktcTime.LocalTimeOfDay(request.ReturnAt)))
        {
            lines.Add(Line("OUT_OF_HOURS_RETURN", "Mesai dışı iade", 1, policy.OutOfHoursFee));
        }

        return new QuoteDto(request.CategoryId, days, PricingDefaults.Currency, lines, lines.Sum(l => l.Amount), inputs.Deposit);
    }

    public static decimal SeasonMultiplier(IReadOnlyList<Season> seasons, DateOnly date) =>
        seasons.Where(s => s.Contains(date)).Select(s => s.Multiplier).DefaultIfEmpty(1m).Max();

    private static QuoteLineDto ExtraLine(QuoteExtraRequest selection, IReadOnlyDictionary<string, Extra> extras, int days)
    {
        if (!extras.TryGetValue(Extra.NormalizeCode(selection.Code), out var extra) || !extra.IsActive)
        {
            throw new DomainException("pricing.unknown_extra", $"Bilinmeyen ekstra: {selection.Code}");
        }

        if (selection.Quantity < 1 || selection.Quantity > extra.MaxQuantity)
        {
            throw new DomainException("pricing.extra_quantity", $"{extra.Name} için adet 1–{extra.MaxQuantity} olmalı.");
        }

        if (extra.Unit == ExtraUnit.PerRental)
        {
            return Line(extra.Code, extra.Name, selection.Quantity, extra.Price);
        }

        var chargedDays = Math.Min(days, extra.MaxChargeDays ?? days);
        var amount = Round(extra.Price * chargedDays * selection.Quantity);
        return new QuoteLineDto(extra.Code, $"{extra.Name} · {chargedDays} gün", selection.Quantity, extra.Price, amount);
    }

    private static QuoteLineDto Line(string code, string description, int quantity, decimal unitPrice) =>
        new(code, description, quantity, unitPrice, Round(unitPrice * quantity));

    private static decimal Round(decimal value) => Math.Round(value, 0, MidpointRounding.AwayFromZero);
}
