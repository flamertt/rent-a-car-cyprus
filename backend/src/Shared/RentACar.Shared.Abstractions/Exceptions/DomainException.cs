namespace RentACar.Shared.Abstractions.Exceptions;

/// <summary>İş kuralı ihlali → HTTP 422. <see cref="Code"/> istemcinin çevireceği sabit anahtardır.</summary>
public class DomainException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
