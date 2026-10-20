namespace RentACar.Shared.Abstractions.Exceptions;

/// <summary>Kaynağın durumu işleme izin vermiyor (örn. araç dolu) → HTTP 409.</summary>
public sealed class ConflictException(string code, string message) : DomainException(code, message);
