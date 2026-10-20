using RentACar.Modules.Pricing.Application.Queries.CalculateQuotes;
using RentACar.Modules.Pricing.Application.Queries.ListExtras;
using RentACar.Modules.Pricing.Contracts;
using RentACar.Modules.Pricing.Contracts.Dtos;
using RentACar.Modules.Pricing.Contracts.Requests;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Services;

internal sealed class PricingModuleApi(IDispatcher dispatcher) : IPricingApi
{
    public async Task<QuoteDto> QuoteAsync(QuoteRequest request, CancellationToken cancellationToken = default)
    {
        var quotes = await QuoteManyAsync(request, [request.CategoryId], cancellationToken);
        return quotes.TryGetValue(request.CategoryId, out var quote)
            ? quote
            : throw new DomainException("pricing.no_rate", "Bu araç sınıfı için tarife tanımlı değil.");
    }

    public Task<IReadOnlyDictionary<Guid, QuoteDto>> QuoteManyAsync(
        QuoteRequest template, IReadOnlyCollection<Guid> categoryIds, CancellationToken cancellationToken = default) =>
        dispatcher.QueryAsync(new CalculateQuotesQuery(template, categoryIds), cancellationToken);

    public Task<IReadOnlyList<ExtraDto>> ListExtrasAsync(CancellationToken cancellationToken = default) =>
        dispatcher.QueryAsync(new ListExtrasQuery(), cancellationToken);
}
