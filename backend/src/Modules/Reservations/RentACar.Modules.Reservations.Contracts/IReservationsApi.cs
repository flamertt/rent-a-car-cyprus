using RentACar.Modules.Reservations.Contracts.Dtos;
using RentACar.Modules.Reservations.Contracts.Enums;
using RentACar.Modules.Reservations.Contracts.Requests;

namespace RentACar.Modules.Reservations.Contracts;

/// <summary>Rezervasyon modülünün diğer modüllere (Simulation) açtığı tek yüz.</summary>
public interface IReservationsApi
{
    Task<ReservationDto> CreateAsync(CreateReservationRequest request, CancellationToken cancellationToken = default);

    Task<ReservationDto> PickUpAsync(Guid reservationId, int odometerKm, double fuelLevel, CancellationToken cancellationToken = default);

    Task<ReservationDto> ReturnAsync(Guid reservationId, Guid? returnBranchId, int odometerKm, double fuelLevel, CancellationToken cancellationToken = default);

    Task<ReservationDto> CancelAsync(Guid reservationId, CancellationToken cancellationToken = default);

    /// <summary>Verilen durumda olup zamanı gelmiş (teslim / iade) rezervasyonlar.</summary>
    Task<IReadOnlyList<ReservationDto>> ListDueAsync(ReservationStatus status, DateTimeOffset dueBefore, int max, CancellationToken cancellationToken = default);
}
