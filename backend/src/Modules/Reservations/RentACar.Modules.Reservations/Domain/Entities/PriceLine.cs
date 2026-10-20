namespace RentACar.Modules.Reservations.Domain.Entities;

/// <summary>Fiyat satırı anlık görüntüsü (jsonb). Tarife sonradan değişse de rezervasyon tutarı sabit kalır.</summary>
internal sealed class PriceLine
{
    public string Code { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Amount { get; set; }
}
