using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Simulation.Application.Queries.ListRecentEvents;

internal sealed record ListRecentEventsQuery(int Limit) : IQuery<IReadOnlyList<SimEventDto>>;
