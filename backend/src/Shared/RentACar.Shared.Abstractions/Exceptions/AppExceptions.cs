namespace RentACar.Shared.Abstractions.Exceptions;

/// <summary>İş kuralı ihlali → HTTP 422. <see cref="Code"/> istemcinin çevireceği sabit anahtardır.</summary>
public class DomainException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

/// <summary>Kaynağın durumu işleme izin vermiyor (örn. araç dolu) → HTTP 409.</summary>
public sealed class ConflictException(string code, string message) : DomainException(code, message);

public sealed class NotFoundException(string resource, object key)
    : Exception($"{resource} bulunamadı: {key}")
{
    public string Resource { get; } = resource;
}
