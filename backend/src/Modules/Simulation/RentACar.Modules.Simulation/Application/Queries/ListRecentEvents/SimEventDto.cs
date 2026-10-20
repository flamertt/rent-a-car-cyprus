namespace RentACar.Modules.Simulation.Application.Queries.ListRecentEvents;

internal sealed record SimEventDto(DateTimeOffset At, string Kind, string Message, Guid? VehicleId, string? ReservationCode);
