using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Reservations.Application.Abstractions;
using RentACar.Modules.Reservations.Application.Mappings;
using RentACar.Modules.Reservations.Contracts.Dtos;
using RentACar.Shared.Abstractions.Common;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Reservations.Application.Queries.ListReservations;

internal sealed class ListReservationsHandler(IReservationsDbContext db) : IQueryHandler<ListReservationsQuery, PagedResult<ReservationDto>>
{
    public async Task<PagedResult<ReservationDto>> HandleAsync(ListReservationsQuery q, CancellationToken cancellationToken)
    {
        var (page, size) = Paging.Normalize(q.Page, q.PageSize);
        var reservations = db.Reservations.AsNoTracking();
        if (q.Status is { } status)
        {
            reservations = reservations.Where(r => r.Status == status);
        }

        if (q.Channel is { } channel)
        {
            reservations = reservations.Where(r => r.Channel == channel);
        }

        if (q.BranchId is { } branchId)
        {
            reservations = reservations.Where(r => r.PickupBranchId == branchId || r.ReturnBranchId == branchId);
        }

        // Aralık kesişimi: [from, to) ile [PickupAt, ReturnAt) örtüşenler.
        if (q.From is { } from)
        {
            var f = from.ToUniversalTime();
            reservations = reservations.Where(r => r.ReturnAt > f);
        }

        if (q.To is { } to)
        {
            var t = to.ToUniversalTime();
            reservations = reservations.Where(r => r.PickupAt < t);
        }

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var pattern = $"%{q.Search.Trim()}%";
            reservations = reservations.Where(r => EF.Functions.ILike(r.Code, pattern)
                || EF.Functions.ILike(r.CustomerName, pattern)
                || EF.Functions.ILike(r.VehiclePlate, pattern));
        }

        var total = await reservations.CountAsync(cancellationToken);
        var rows = await reservations.OrderByDescending(r => r.PickupAt).Skip((page - 1) * size).Take(size).ToListAsync(cancellationToken);
        return new PagedResult<ReservationDto>(rows.Select(r => r.ToDto()).ToList(), page, size, total);
    }
}
