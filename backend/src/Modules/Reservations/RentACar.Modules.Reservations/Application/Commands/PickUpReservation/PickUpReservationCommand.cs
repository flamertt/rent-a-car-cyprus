using RentACar.Modules.Reservations.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Reservations.Application.Commands.PickUpReservation;

/// <summary>Aracı müşteriye teslim et; sayaçlar boşsa aracın mevcut değerleri alınır.</summary>
internal sealed record PickUpReservationCommand(Guid ReservationId, int? OdometerKm, double? FuelLevel) : ICommand<ReservationDto>;
