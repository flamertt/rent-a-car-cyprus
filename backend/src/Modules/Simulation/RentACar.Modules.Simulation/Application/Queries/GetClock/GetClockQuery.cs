using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Simulation.Application.Queries.GetClock;

internal sealed record GetClockQuery : IQuery<ClockDto>;
