using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RentACar.Modules.Fleet.Contracts;
using RentACar.Modules.Fleet.Contracts.Dtos;
using RentACar.Modules.Reservations.Application.Abstractions;
using RentACar.Modules.Reservations.Contracts.Enums;
using RentACar.Modules.Reservations.Domain.Rules;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Reservations.Application.Services;

/// <summary>Takvim: bir aralıkta hangi araçlar boş (Fleet adayları − örtüşen rezervasyonlar).</summary>
internal sealed class VehicleAvailability(IReservationsDbContext db, IFleetApi fleet, IClock clock, IOptions<ReservationRules> rules)
{
    /// <summary>Teslim şubesinde duran araç önce (transfer gerekmez), sonra az kilometreli.</summary>
    public async Task<IReadOnlyList<RentableVehicleDto>> FreeVehiclesAsync(
        Guid categoryId, Guid pickupBranchId, DateTimeOffset pickupAt, DateTimeOffset returnAt, CancellationToken ct)
    {
        var candidates = await fleet.ListRentableVehiclesAsync(categoryId, ct);
        var busy = await BusyVehicleIdsAsync(candidates.Select(v => v.Id).ToList(), pickupAt, returnAt, ct);
        return candidates
            .Where(v => !busy.Contains(v.Id))
            .OrderByDescending(v => v.BranchId == pickupBranchId)
            .ThenBy(v => v.OdometerKm)
            .ToList();
    }

    /// <summary>
    /// [pickupAt, returnAt + hazırlık) aralığıyla örtüşen onaylı/aktif rezervasyonların araçları; ayrıca yakın bir
    /// teslim isteniyorsa iadesi gecikmiş (hâlâ müşteride) araçlar.
    /// </summary>
    public async Task<HashSet<Guid>> BusyVehicleIdsAsync(
        IReadOnlyCollection<Guid> vehicleIds, DateTimeOffset pickupAt, DateTimeOffset returnAt, CancellationToken ct)
    {
        if (vehicleIds.Count == 0)
        {
            return [];
        }

        var now = clock.UtcNow;
        var blockedUntil = (returnAt + rules.Value.Turnaround).ToUniversalTime();
        var from = pickupAt.ToUniversalTime();
        var overdueMatters = from < now.AddDays(1);
        var ids = vehicleIds.ToList();

        var busy = await db.Reservations.AsNoTracking()
            .Where(r => ids.Contains(r.VehicleId))
            .Where(r =>
                ((r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.Active)
                    && r.PickupAt < blockedUntil && from < r.BlockedUntil)
                || (overdueMatters && r.Status == ReservationStatus.Active && r.BlockedUntil <= now))
            .Select(r => r.VehicleId)
            .Distinct()
            .ToListAsync(ct);
        return busy.ToHashSet();
    }
}
