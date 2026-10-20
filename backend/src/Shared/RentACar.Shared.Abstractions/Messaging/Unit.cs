namespace RentACar.Shared.Abstractions.Messaging;

/// <summary>Sonuç döndürmeyen komutların sonucu.</summary>
public readonly record struct Unit
{
    public static readonly Unit Value = default;
}
