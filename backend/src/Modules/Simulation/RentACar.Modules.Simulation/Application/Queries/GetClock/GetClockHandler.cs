using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Simulation.Application.Queries.GetClock;

internal sealed class GetClockHandler(IClock clock) : IQueryHandler<GetClockQuery, ClockDto>
{
    public Task<ClockDto> HandleAsync(GetClockQuery query, CancellationToken cancellationToken) =>
        Task.FromResult(new ClockDto(clock.UtcNow, clock.Speed));
}
