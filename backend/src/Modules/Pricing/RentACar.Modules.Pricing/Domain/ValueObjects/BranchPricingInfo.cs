namespace RentACar.Modules.Pricing.Domain.ValueObjects;

/// <summary>Fiyat hesabının bir şubeden ihtiyaç duyduğu bilgi (Branches modülünden uygulama katmanında eşlenir).</summary>
internal sealed record BranchPricingInfo(string Name, string District, decimal DeliveryFee, TimeOnly OpensAt, TimeOnly ClosesAt)
{
    /// <summary>Gece yarısını aşan çalışma saatleri (örn. havalimanı 00:00–23:59) desteklenir.</summary>
    public bool IsOpenAt(TimeOnly localTime) =>
        OpensAt <= ClosesAt ? localTime >= OpensAt && localTime <= ClosesAt : localTime >= OpensAt || localTime <= ClosesAt;
}
