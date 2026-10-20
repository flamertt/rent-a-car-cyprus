using RentACar.Modules.Customers.Contracts.Dtos;
using RentACar.Shared.Abstractions.Common;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Customers.Application.Queries.ListCustomers;

internal sealed record ListCustomersQuery(string? Search, string? Nationality, bool? Simulated, int? Page, int? PageSize)
    : IQuery<PagedResult<CustomerDto>>;
