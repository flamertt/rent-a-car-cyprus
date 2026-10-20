using RentACar.Modules.Pricing.Contracts.Dtos;
using RentACar.Modules.Pricing.Contracts.Requests;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Pricing.Application.Queries.CalculateQuotes;

/// <summary>Aynı tarih/şube/ekstra şablonuyla birden çok sınıfa teklif.</summary>
internal sealed record CalculateQuotesQuery(QuoteRequest Template, IReadOnlyCollection<Guid> CategoryIds)
    : IQuery<IReadOnlyDictionary<Guid, QuoteDto>>;
