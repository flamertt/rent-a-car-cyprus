using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using RentACar.Modules.Reservations.Application.Commands.CancelReservation;
using RentACar.Modules.Reservations.Application.Commands.PickUpReservation;
using RentACar.Modules.Reservations.Application.Commands.ReturnReservation;
using RentACar.Modules.Reservations.Application.Queries.GetReservation;
using RentACar.Modules.Reservations.Application.Queries.GetReservationStats;
using RentACar.Modules.Reservations.Application.Queries.GetReservationTimeline;
using RentACar.Modules.Reservations.Application.Queries.ListReservations;
using RentACar.Modules.Reservations.Contracts.Dtos;
using RentACar.Modules.Reservations.Contracts.Enums;
using RentACar.Modules.Reservations.Endpoints.Requests;
using RentACar.Shared.Abstractions.Messaging;
using RentACar.Shared.Infrastructure.Endpoints;

namespace RentACar.Modules.Reservations.Endpoints;

/// <summary>
/// Yalnız yönetim paneli: rezervasyonları simülasyon üretir (müşteri sitesi yok); panel izler ve operasyon
/// adımlarını (teslim, iade, iptal) elle de yapabilir.
/// </summary>
internal static class AdminReservationEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapAdminGroup("reservations", "Rezervasyon");

        group.MapGet("/", (ReservationStatus? status, BookingChannel? channel, Guid? branchId, DateTimeOffset? from, DateTimeOffset? to,
                string? search, int? page, int? pageSize, IDispatcher d, CancellationToken ct) =>
                d.QueryAsync(new ListReservationsQuery(status, channel, branchId, from, to, search, page, pageSize), ct))
            .WithSummary("Filtreli, sayfalı liste");
        group.MapGet("/{id:guid}", GetAsync);
        group.MapPost("/{id:guid}/pickup", (Guid id, PickUpRequest r, IDispatcher d, CancellationToken ct) =>
                d.SendAsync(new PickUpReservationCommand(id, r.OdometerKm, r.FuelLevel), ct))
            .WithSummary("Aracı müşteriye teslim et");
        group.MapPost("/{id:guid}/return", (Guid id, ReturnRequest r, IDispatcher d, CancellationToken ct) =>
                d.SendAsync(new ReturnReservationCommand(id, r.ReturnBranchId, r.OdometerKm, r.FuelLevel), ct))
            .WithSummary("İade al (geç iade / eksik yakıt hesaplanır)");
        group.MapPost("/{id:guid}/cancel", (Guid id, IDispatcher d, CancellationToken ct) => d.SendAsync(new CancelReservationCommand(id), ct));
        group.MapGet("/stats", (int? days, IDispatcher d, CancellationToken ct) => d.QueryAsync(new GetReservationStatsQuery(days), ct))
            .WithSummary("Pano: durum, kanal, günlük ciro, sınıf bazında gelir");
        group.MapGet("/timeline", (DateTimeOffset? from, DateTimeOffset? to, IDispatcher d, CancellationToken ct) =>
                d.QueryAsync(new GetReservationTimelineQuery(from, to), ct))
            .WithSummary("Araç bazlı takvim (Gantt) için aralık sorgusu");
    }

    private static async Task<Results<Ok<ReservationDto>, NotFound>> GetAsync(Guid id, IDispatcher dispatcher, CancellationToken ct) =>
        await dispatcher.QueryAsync(new GetReservationQuery(id), ct) is { } reservation ? TypedResults.Ok(reservation) : TypedResults.NotFound();
}
