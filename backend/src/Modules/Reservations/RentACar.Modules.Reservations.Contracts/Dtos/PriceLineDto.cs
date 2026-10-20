namespace RentACar.Modules.Reservations.Contracts.Dtos;

public sealed record PriceLineDto(string Code, string Description, int Quantity, decimal UnitPrice, decimal Amount);
