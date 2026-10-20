using RentACar.Modules.Reservations.Contracts.Dtos;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Reservations.Application.Commands.ReturnReservation;

/// <summary>İade al; <paramref name="ReturnBranchId"/> boşsa planlanan iade şubesi.</summary>
internal sealed record ReturnReservationCommand(Guid ReservationId, Guid? ReturnBranchId, int OdometerKm, double FuelLevel)
    : ICommand<ReservationDto>;
