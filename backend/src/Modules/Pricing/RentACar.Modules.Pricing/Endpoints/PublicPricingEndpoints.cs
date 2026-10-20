using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RentACar.Modules.Pricing.Application.Queries.ListExtras;
using RentACar.Modules.Pricing.Contracts;
using RentACar.Modules.Pricing.Contracts.Requests;
using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Infrastructure.Endpoints;

namespace RentACar.Modules.Pricing.Endpoints;

internal static class PublicPricingEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapPublicGroup("pricing", "Fiyatlandırma");
        group.MapGet("/extras", (IDispatcher d, CancellationToken ct) => d.QueryAsync(new ListExtrasQuery(), ct));
        group.MapPost("/quote", (QuoteRequest request, IPricingApi pricing, CancellationToken ct) => pricing.QuoteAsync(request, ct))
            .WithSummary("Fiyat teklifi: sezon, süre indirimi, ekstralar, teslim/tek yön/mesai dışı ücretleri");
    }
}
