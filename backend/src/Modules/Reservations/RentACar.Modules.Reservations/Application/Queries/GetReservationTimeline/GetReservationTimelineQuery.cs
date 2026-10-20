using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Reservations.Application.Queries.GetReservationTimeline;

/// <summary>Araç bazlı takvim (Gantt); varsayılan pencere: şimdi −2 gün … +12 gün.</summary>
internal sealed record GetReservationTimelineQuery(DateTimeOffset? From, DateTimeOffset? To) : IQuery<IReadOnlyList<TimelineItemDto>>;
