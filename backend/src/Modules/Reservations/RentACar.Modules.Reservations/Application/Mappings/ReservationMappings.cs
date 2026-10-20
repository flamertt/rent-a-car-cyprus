using RentACar.Modules.Reservations.Contracts.Dtos;
using RentACar.Modules.Reservations.Domain.Entities;

namespace RentACar.Modules.Reservations.Application.Mappings;

internal static class ReservationMappings
{
    public static ReservationDto ToDto(this Reservation r) => new(
        r.Id, r.Code, r.Status, r.Channel, r.CustomerId, r.CustomerName, r.CategoryId, r.CategoryCode,
        r.VehicleId, r.VehiclePlate, r.PickupBranchId, r.ReturnBranchId, r.PickupAt, r.ReturnAt,
        r.PickedUpAt, r.ReturnedAt, r.Days, r.Total, r.Currency,
        r.Lines.Select(l => new PriceLineDto(l.Code, l.Description, l.Quantity, l.UnitPrice, l.Amount)).ToList(),
        r.CreatedAt);
}
