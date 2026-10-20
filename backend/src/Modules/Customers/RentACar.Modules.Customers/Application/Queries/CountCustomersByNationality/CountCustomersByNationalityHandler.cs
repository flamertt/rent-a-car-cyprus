using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Customers.Application.Abstractions;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Customers.Application.Queries.CountCustomersByNationality;

internal sealed class CountCustomersByNationalityHandler(ICustomersDbContext db)
    : IQueryHandler<CountCustomersByNationalityQuery, IReadOnlyDictionary<string, int>>
{
    public async Task<IReadOnlyDictionary<string, int>> HandleAsync(CountCustomersByNationalityQuery query, CancellationToken cancellationToken) =>
        await db.Customers
            .GroupBy(c => c.Nationality)
            .Select(g => new { Nationality = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToDictionaryAsync(x => x.Nationality, x => x.Count, cancellationToken);
}
