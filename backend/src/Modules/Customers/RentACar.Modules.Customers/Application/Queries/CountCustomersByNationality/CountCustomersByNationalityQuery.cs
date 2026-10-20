using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Customers.Application.Queries.CountCustomersByNationality;

internal sealed record CountCustomersByNationalityQuery : IQuery<IReadOnlyDictionary<string, int>>;
