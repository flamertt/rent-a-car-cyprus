using System.Globalization;
using Microsoft.Extensions.Options;
using RentACar.Modules.Branches.Contracts.Dtos;
using RentACar.Modules.Branches.Contracts.Enums;
using RentACar.Modules.Reservations.Contracts;
using RentACar.Modules.Reservations.Contracts.Enums;
using RentACar.Modules.Reservations.Contracts.Requests;
using RentACar.Modules.Simulation.Application.Options;
using RentACar.Modules.Simulation.Domain.Entities;
using RentACar.Shared.Abstractions.Exceptions;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Simulation.Application.Engine;

/// <summary>
/// Rastgele müşteri talebi → gerçek rezervasyon (Reservations modülünün kuralları aynen geçerli).
/// Günlük talep = filo × aylık doluluk ÷ ortalama kiralama süresi; doluluk Turizm Planlama Dairesi 2026
/// verisinden (Ağustos %69, Şubat–Mart ~%27, yıl ortalaması %43; docs/KKTC-VERI-KAYNAKLARI.md §5.2).
/// Kiralamaların çoğu Ercan'dan başlar (havayolu girişlerinin %96'sı), bir kısmı Larnaka'dan gelenler için sınır kapılarından.
/// </summary>
internal sealed class DemandGenerator(IReservationsApi reservations, IOptions<SimulationOptions> options, EventLog log)
{
    private const double AverageRentalDays = 5.5;

    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

    private static readonly double[] MonthlyOccupancy = [0, 0.25, 0.27, 0.27, 0.38, 0.45, 0.52, 0.62, 0.69, 0.55, 0.45, 0.33, 0.30];

    private static readonly Dictionary<string, double> PickupWeights = new()
    {
        ["ECN-DSK"] = 55, ["GRN-OFC"] = 13, ["LEF-OFC"] = 9, ["MAG-OFC"] = 6,
        ["MTH-BRD"] = 9, ["GRN-PRT"] = 4, ["BYM-BRD"] = 2, ["AKY-BRD"] = 2,
    };

    private static readonly Dictionary<string, double> CategoryWeights = new()
    {
        ["MCAR"] = 14, ["ECAR"] = 30, ["CDAR"] = 12, ["CDAH"] = 12, ["CFAR"] = 10,
        ["IFAR"] = 6, ["IVAR"] = 7, ["PDAR"] = 4, ["STAR"] = 5,
    };

    private static readonly (int Days, double Weight)[] Durations =
        [(1, 5), (2, 8), (3, 14), (4, 14), (5, 14), (6, 9), (7, 16), (10, 8), (14, 9), (21, 3)];

    public async Task RunAsync(SimContext ctx, SimState state, int fleetSize, CancellationToken ct)
    {
        var elapsedDays = Math.Clamp((ctx.Now - state.LastDemandAt).TotalDays, 0, 2);
        state.LastDemandAt = ctx.Now;
        if (elapsedDays <= 0 || fleetSize == 0)
        {
            return;
        }

        var month = KktcTime.ToLocal(ctx.Now).Month;
        var perDay = fleetSize * MonthlyOccupancy[month] / AverageRentalDays * 1.15 * options.Value.DemandMultiplier;
        var count = Math.Min(20, ctx.Rng.Poisson(perDay * elapsedDays));
        for (var i = 0; i < count; i++)
        {
            await BookOneAsync(ctx, state, ct);
        }
    }

    private async Task BookOneAsync(SimContext ctx, SimState state, CancellationToken ct)
    {
        var rng = ctx.Rng;
        var pickupBranch = rng.Pick(ctx.Branches, b => PickupWeights.GetValueOrDefault(b.Code, 1));
        var pickupAt = PickupTime(ctx, pickupBranch);
        var days = rng.Pick(Durations, d => d.Weight).Days;
        var returnBranch = ReturnBranch(ctx, pickupBranch);
        var returnLocal = KktcTime.ToLocal(pickupAt).AddDays(days).AddMinutes(rng.Next(-8, 5) * 15);
        var returnAt = ctx.LocalToUtc(DateOnly.FromDateTime(returnLocal.DateTime), Math.Clamp(returnLocal.Hour, 7, 21), returnLocal.Minute / 15 * 15);

        var (customer, age) = CustomerFactory.Create(rng, KktcTime.LocalDate(pickupAt));
        var category = rng.Pick(ctx.Categories.Where(c => c.MinDriverAge <= age).ToList(), c => CategoryWeights.GetValueOrDefault(c.Code, 3));

        try
        {
            var r = await reservations.CreateAsync(new CreateReservationRequest(
                null, customer, category.Id, pickupBranch.Id, returnBranch.Id, pickupAt, returnAt,
                Extras(rng, pickupBranch), BookingChannel.Simulation), ct);
            state.ReservationsCreated++;
            log.Write(ctx.Now, "reservation",
                $"{r.Code}: {r.CustomerName} ({customer.Nationality}) · {category.Name} · {pickupBranch.Name} → {returnBranch.Name} · " +
                $"{KktcTime.ToLocal(pickupAt).ToString("d MMM HH:mm", Tr)} · {r.Days} gün · {r.Total.ToString("N0", Tr)} TL",
                r.VehicleId, r.Code);
        }
        catch (ConflictException ex) when (ex.Code == "reservation.sold_out")
        {
            state.LostDemand++;
            log.Write(ctx.Now, "lost", $"Talep karşılanamadı: {category.Name}, {pickupBranch.Name}, {KktcTime.ToLocal(pickupAt).ToString("d MMM", Tr)} ({days} gün) — boş araç yok");
        }
        catch (DomainException ex)
        {
            log.Write(ctx.Now, "rejected", $"Rezervasyon reddedildi ({ex.Code}): {ex.Message}");
        }
    }

    private static List<ReservationExtraRequest> Extras(Random rng, BranchDto pickupBranch)
    {
        var extras = new List<ReservationExtraRequest>();
        if (rng.Chance(0.30))
        {
            extras.Add(new ReservationExtraRequest("FULL_COVER", 1));
        }

        if (rng.Chance(0.10))
        {
            extras.Add(new ReservationExtraRequest("BABY_SEAT", 1));
        }

        if (rng.Chance(0.08))
        {
            extras.Add(new ReservationExtraRequest("EXTRA_DRIVER", 1));
        }

        if (pickupBranch.Type == BranchType.AirportDesk && rng.Chance(0.15))
        {
            extras.Add(new ReservationExtraRequest("AIRPORT_MEET", 1));
        }

        return extras;
    }

    /// <summary>Teslim anı: %20 aynı gün, %50 bir hafta içinde, %30 1–4 hafta sonra; saat şube tipine göre.</summary>
    private static DateTimeOffset PickupTime(SimContext ctx, BranchDto branch)
    {
        var rng = ctx.Rng;
        var roll = rng.NextDouble();
        var leadHours = roll < 0.2 ? rng.Next(3, 12) : roll < 0.7 ? rng.Next(24, 24 * 7) : rng.Next(24 * 7, 24 * 28);
        var day = KktcTime.LocalDate(ctx.Now.AddHours(leadHours));
        var hour = branch.Type switch
        {
            BranchType.Office => rng.Next(9, 18),
            BranchType.PortDesk => rng.Next(8, 20),
            _ => rng.Pick(Enumerable.Range(0, 24).ToList(), h => h is >= 7 and <= 22 ? 3 : 1), // havalimanı/kapı: gece uçuşları da var
        };
        var at = ctx.LocalToUtc(day, hour, rng.Next(0, 4) * 15);
        var earliest = ctx.Now.AddMinutes(90);
        return at < earliest ? ctx.LocalToUtc(KktcTime.LocalDate(earliest), KktcTime.ToLocal(earliest).Hour, 0).AddHours(1) : at;
    }

    /// <summary>Ercan'dan alanlar çoğunlukla Ercan'a, sınır kapısından alanlar aynı kapıya döner (uçuş Larnaka'dan).</summary>
    private static BranchDto ReturnBranch(SimContext ctx, BranchDto pickup)
    {
        var sameProbability = pickup.Type switch
        {
            BranchType.AirportDesk => 0.80,
            BranchType.BorderCrossing => 0.85,
            BranchType.PortDesk => 0.70,
            _ => 0.75,
        };
        return ctx.Rng.Chance(sameProbability) ? pickup : ctx.Rng.Pick(ctx.StockBranches, b => b.Type == BranchType.AirportDesk ? 3 : 1);
    }
}
