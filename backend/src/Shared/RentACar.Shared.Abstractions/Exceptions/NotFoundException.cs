namespace RentACar.Shared.Abstractions.Exceptions;

/// <summary>Kaynak yok → HTTP 404.</summary>
public sealed class NotFoundException(string resource, object key)
    : Exception($"{resource} bulunamadı: {key}")
{
    public string Resource { get; } = resource;
}
