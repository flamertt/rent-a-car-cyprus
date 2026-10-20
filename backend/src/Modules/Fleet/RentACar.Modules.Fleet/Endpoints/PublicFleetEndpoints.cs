using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RentACar.Modules.Fleet.Application.Queries.ListCategories;
using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Infrastructure.Endpoints;

namespace RentACar.Modules.Fleet.Endpoints;

internal static class PublicFleetEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapPublicGroup("fleet", "Filo");
        group.MapGet("/categories", (IDispatcher d, CancellationToken ct) => d.QueryAsync(new ListCategoriesQuery(), ct))
            .WithSummary("Kiralama sınıfları (ACRISS)");
    }
}
