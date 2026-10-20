namespace RentACar.Modules.Simulation.Application.Queries.ListActiveTrips;

/// <summary><see cref="Route"/>: yoldaysa rota [lon, lat] noktaları (en çok ~300), park hâlindeyse boş.</summary>
internal sealed record ActiveTripDto(
    Guid VehicleId,
    string Plate,
    string? ReservationCode,
    string? CustomerName,
    string Kind,
    string Phase,
    string DestinationName,
    string? StayName,
    double[][] Route,
    double Progress,
    DateTimeOffset DepartAt,
    DateTimeOffset? ReturnAt);
