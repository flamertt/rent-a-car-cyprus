using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Simulation.Application.Queries.ListActiveTrips;

internal sealed record ListActiveTripsQuery : IQuery<IReadOnlyList<ActiveTripDto>>;
