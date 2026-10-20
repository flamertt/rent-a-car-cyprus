namespace RentACar.Modules.Simulation.Application.Queries.GetSimulationStatus;

internal sealed record SimulationStatusDto(
    bool Running,
    double Speed,
    bool CatchingUp,
    DateTimeOffset Now,
    int ActiveTrips,
    int ReservationsCreated,
    int LostDemand,
    int Pickups,
    int Returns,
    string? LastError,
    string? OsrmUrl,
    bool RoadRouting);
