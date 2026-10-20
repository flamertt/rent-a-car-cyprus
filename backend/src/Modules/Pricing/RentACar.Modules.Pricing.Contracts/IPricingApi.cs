using RentACar.Modules.Pricing.Contracts.Dtos;
using RentACar.Modules.Pricing.Contracts.Requests;

namespace RentACar.Modules.Pricing.Contracts;

/// <summary>Fiyatlandırma modülünün diğer modüllere açtığı tek yüz.</summary>
public interface IPricingApi
{
    Task<QuoteDto> QuoteAsync(QuoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Aynı tarih/şube/ekstra için birden çok sınıfa teklif; tarifesi olmayan sınıf atlanır.</summary>
    Task<IReadOnlyDictionary<Guid, QuoteDto>> QuoteManyAsync(
        QuoteRequest template, IReadOnlyCollection<Guid> categoryIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExtraDto>> ListExtrasAsync(CancellationToken cancellationToken = default);
}
