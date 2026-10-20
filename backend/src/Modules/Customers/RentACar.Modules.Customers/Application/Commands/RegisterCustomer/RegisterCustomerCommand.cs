using RentACar.Modules.Customers.Contracts.Dtos;
using RentACar.Modules.Customers.Contracts.Requests;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Customers.Application.Commands.RegisterCustomer;

/// <summary>E-posta anahtarıyla ekle/güncelle.</summary>
internal sealed record RegisterCustomerCommand(RegisterCustomerRequest Request) : ICommand<CustomerDto>;
