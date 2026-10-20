using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Simulation.Application.Abstractions;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Simulation.Application.Queries.ListRecentEvents;

internal sealed class ListRecentEventsHandler(ISimulationDbContext db) : IQueryHandler<ListRecentEventsQuery, IReadOnlyList<SimEventDto>>
{
    public async Task<IReadOnlyList<SimEventDto>> HandleAsync(ListRecentEventsQuery query, CancellationToken cancellationToken) =>
        await db.Events.AsNoTracking()
            .OrderByDescending(e => e.Id)
            .Take(Math.Clamp(query.Limit, 1, 500))
            .Select(e => new SimEventDto(e.At, e.Kind, e.Message, e.VehicleId, e.ReservationCode))
            .ToListAsync(cancellationToken);
}
