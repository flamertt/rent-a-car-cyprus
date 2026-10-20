using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Customers.Application.Abstractions;
using RentACar.Modules.Customers.Application.Mappings;
using RentACar.Modules.Customers.Contracts.Dtos;
using RentACar.Shared.Abstractions.Common;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Customers.Application.Queries.ListCustomers;

internal sealed class ListCustomersHandler(ICustomersDbContext db) : IQueryHandler<ListCustomersQuery, PagedResult<CustomerDto>>
{
    public async Task<PagedResult<CustomerDto>> HandleAsync(ListCustomersQuery q, CancellationToken cancellationToken)
    {
        var (page, size) = Paging.Normalize(q.Page, q.PageSize);
        var customers = db.Customers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var pattern = $"%{q.Search.Trim()}%";
            customers = customers.Where(c =>
                EF.Functions.ILike(c.FirstName + " " + c.LastName, pattern)
                || EF.Functions.ILike(c.Email, pattern)
                || EF.Functions.ILike(c.LicenseNumber, pattern));
        }

        if (!string.IsNullOrWhiteSpace(q.Nationality))
        {
            var code = q.Nationality.ToUpperInvariant();
            customers = customers.Where(c => c.Nationality == code);
        }

        if (q.Simulated is { } simulated)
        {
            customers = customers.Where(c => c.IsSimulated == simulated);
        }

        var total = await customers.CountAsync(cancellationToken);
        var items = await customers.OrderByDescending(c => c.CreatedAt).Skip((page - 1) * size).Take(size).ToListAsync(cancellationToken);
        return new PagedResult<CustomerDto>(items.Select(c => c.ToDto()).ToList(), page, size, total);
    }
}
