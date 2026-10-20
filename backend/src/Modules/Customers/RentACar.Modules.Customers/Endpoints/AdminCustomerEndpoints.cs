using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using RentACar.Modules.Customers.Application.Queries.CountCustomersByNationality;
using RentACar.Modules.Customers.Application.Queries.GetCustomer;
using RentACar.Modules.Customers.Application.Queries.ListCustomers;
using RentACar.Modules.Customers.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Infrastructure.Endpoints;

namespace RentACar.Modules.Customers.Endpoints;

/// <summary>Yalnız yönetim paneli: müşteriler simülasyonun rezervasyonlarıyla birlikte oluşur.</summary>
internal static class AdminCustomerEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapAdminGroup("customers", "Müşteriler");
        group.MapGet("/", (string? search, string? nationality, bool? simulated, int? page, int? pageSize, IDispatcher d, CancellationToken ct) =>
                d.QueryAsync(new ListCustomersQuery(search, nationality, simulated, page, pageSize), ct))
            .WithSummary("Ad/soyad/e-posta/ehliyet araması, uyruk filtresi, sayfalama");
        group.MapGet("/{id:guid}", GetAsync);
        group.MapGet("/by-nationality", (IDispatcher d, CancellationToken ct) => d.QueryAsync(new CountCustomersByNationalityQuery(), ct))
            .WithSummary("Uyruğa göre müşteri sayısı");
    }

    private static async Task<Results<Ok<CustomerDto>, NotFound>> GetAsync(Guid id, IDispatcher dispatcher, CancellationToken ct) =>
        await dispatcher.QueryAsync(new GetCustomerQuery(id), ct) is { } customer ? TypedResults.Ok(customer) : TypedResults.NotFound();
}
