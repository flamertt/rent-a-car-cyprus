using RentACar.Modules.Customers.Application.Commands.RegisterCustomer;
using RentACar.Modules.Customers.Application.Queries.GetCustomer;
using RentACar.Modules.Customers.Contracts;
using RentACar.Modules.Customers.Contracts.Dtos;
using RentACar.Modules.Customers.Contracts.Requests;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Customers.Application.Services;

internal sealed class CustomersModuleApi(IDispatcher dispatcher) : ICustomersApi
{
    public Task<CustomerDto?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        dispatcher.QueryAsync(new GetCustomerQuery(id), cancellationToken);

    public Task<CustomerDto> RegisterAsync(RegisterCustomerRequest request, CancellationToken cancellationToken = default) =>
        dispatcher.SendAsync(new RegisterCustomerCommand(request), cancellationToken);
}
