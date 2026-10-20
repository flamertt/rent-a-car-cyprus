using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Fleet.Application.Queries.GetFleetSummary;

internal sealed record GetFleetSummaryQuery : IQuery<FleetSummaryDto>;
