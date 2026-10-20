using RentACar.Modules.Customers.Contracts;
using RentACar.Modules.Customers.Contracts.Dtos;
using RentACar.Modules.Customers.Contracts.Requests;
using RentACar.Modules.Reservations.Contracts.Enums;
using RentACar.Modules.Reservations.Contracts.Requests;
using RentACar.Shared.Abstractions.Exceptions;

namespace RentACar.Modules.Reservations.Application.Services;

/// <summary>Rezervasyonun müşterisi: kimlikle bulunur ya da bilgileriyle (e-posta anahtarı) kaydedilir.</summary>
internal sealed class CustomerResolver(ICustomersApi customers)
{
    public async Task<CustomerDto> ResolveAsync(CreateReservationRequest request, CancellationToken ct)
    {
        if (request.CustomerId is { } customerId)
        {
            return await customers.GetAsync(customerId, ct) ?? throw new NotFoundException("Müşteri", customerId);
        }

        var c = request.Customer ?? throw new DomainException("reservation.customer_required", "Müşteri bilgisi veya kimliği zorunlu.");
        return await customers.RegisterAsync(new RegisterCustomerRequest(
            c.FirstName, c.LastName, c.Email, c.Phone, c.Nationality, c.DateOfBirth,
            c.LicenseNumber, c.LicenseIssuedOn, c.LicenseCountry, request.Channel == BookingChannel.Simulation), ct);
    }
}
