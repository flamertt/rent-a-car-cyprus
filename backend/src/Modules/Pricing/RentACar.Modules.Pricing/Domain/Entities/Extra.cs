using RentACar.Modules.Pricing.Contracts.Enums;
using RentACar.Shared.Abstractions.Exceptions;

namespace RentACar.Modules.Pricing.Domain.Entities;

internal sealed class Extra
{
    private Extra()
    {
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string Description { get; private set; } = null!;

    public ExtraUnit Unit { get; private set; }

    public decimal Price { get; private set; }

    public int MaxQuantity { get; private set; }

    /// <summary>Günlük ekstralarda ücretlendirilen azami gün (örn. bebek koltuğu 10 günden sonra bedava).</summary>
    public int? MaxChargeDays { get; private set; }

    public bool IsActive { get; private set; }

    public static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();

    public static Extra Create(string code, string name, string description, ExtraUnit unit, decimal price, int maxQuantity, int? maxChargeDays) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            Code = NormalizeCode(code),
            Name = name,
            Description = description,
            Unit = unit,
            Price = price,
            MaxQuantity = maxQuantity,
            MaxChargeDays = maxChargeDays,
            IsActive = true,
        };

    public void Update(string name, string description, decimal price, int maxQuantity, int? maxChargeDays, bool isActive)
    {
        if (price < 0 || maxQuantity < 1)
        {
            throw new DomainException("pricing.invalid_extra", "Ekstra fiyatı negatif, adedi 1'den küçük olamaz.");
        }

        Name = name;
        Description = description;
        Price = price;
        MaxQuantity = maxQuantity;
        MaxChargeDays = maxChargeDays;
        IsActive = isActive;
    }
}
