using RentACar.Modules.Customers.Contracts.Dtos;
using RentACar.Modules.Customers.Contracts.Requests;

namespace RentACar.Modules.Customers.Contracts;

/// <summary>Müşteriler modülünün diğer modüllere açtığı tek yüz.</summary>
public interface ICustomersApi
{
    Task<CustomerDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>E-posta anahtarıyla ekler ya da günceller.</summary>
    Task<CustomerDto> RegisterAsync(RegisterCustomerRequest request, CancellationToken cancellationToken = default);
}
