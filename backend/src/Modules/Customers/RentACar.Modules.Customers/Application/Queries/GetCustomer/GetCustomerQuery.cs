using RentACar.Modules.Customers.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Customers.Application.Queries.GetCustomer;

internal sealed record GetCustomerQuery(Guid Id) : IQuery<CustomerDto?>;
