using RentACar.Modules.Reservations.Contracts.Dtos;
using RentACar.Modules.Reservations.Contracts.Requests;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Reservations.Application.Commands.CreateReservation;

/// <summary>Anında onaylı rezervasyon: kurallar, fiyat, araç ataması.</summary>
internal sealed record CreateReservationCommand(CreateReservationRequest Request) : ICommand<ReservationDto>;
