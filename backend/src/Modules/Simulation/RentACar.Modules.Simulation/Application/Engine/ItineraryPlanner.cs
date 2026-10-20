using RentACar.Modules.Simulation.Domain.Entities;
using RentACar.Modules.Simulation.Domain.Enums;
using RentACar.Modules.Simulation.Domain.ValueObjects;
using RentACar.Shared.Abstractions.Geo;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Simulation.Application.Engine;

/// <summary>
/// Kiralama süresince müşterinin bir sonraki hareketi (tembel planlama, varışta çağrılır):
/// konaklama → (sabah) gezi noktası → 1–3 saat → (gündüz, bazen) yakındaki ikinci nokta → konaklama →
/// (öğleden sonra) ikinci gezi ya da (akşamüstü) akşam yemeği için kasabaya … → iade saatine göre iade şubesi.
/// Gece konaklamaya dönen ertesi sabah çıkar. Müşterilerin ~%10'u 1–4 saat geç döner (geç iade ücretini sınamak için).
/// </summary>
internal static class ItineraryPlanner
{
    private static readonly TimeSpan ReturnBuffer = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan PlannedDwell = TimeSpan.FromHours(2);

    public static void PlanNext(Trip trip, SimContext ctx, DateTimeOffset arrivedAt)
    {
        var rng = ctx.Rng;
        var here = trip.To;
        var returnBranch = ctx.Branch(trip.TargetBranchId) ?? ctx.NearestStock(here);
        var latestDepart = LatestDeparture(trip, here, returnBranch.Location);

        // İade vakti yaklaştı → doğrudan iade şubesine.
        if (arrivedAt.AddHours(2) >= latestDepart)
        {
            trip.PlanLeg(LegPurpose.ToReturnBranch, returnBranch.Name, here, returnBranch.Location, Max(arrivedAt.AddMinutes(15), latestDepart));
            return;
        }

        var stay = trip.Stay ?? here;
        if (trip.Purpose == LegPurpose.Excursion)
        {
            var leaveAt = Min(arrivedAt + TimeSpan.FromMinutes(rng.Next(60, 200)), latestDepart);

            // Gündüz bazen konaklamaya dönmeden yakındaki bir noktaya geçer (gezi turu).
            if (KktcTime.ToLocal(leaveAt).Hour is >= 9 and < 16 && rng.Chance(0.35))
            {
                var next = PickAttraction(ctx, trip, here, 12_000);
                var tour = SimContext.EstimateTravel(here, next.Location) + PlannedDwell + SimContext.EstimateTravel(next.Location, stay);
                if (leaveAt + tour <= latestDepart)
                {
                    trip.LastAttraction = next.Name;
                    trip.PlanLeg(LegPurpose.Excursion, next.Name, here, next.Location, leaveAt);
                    return;
                }
            }

            trip.PlanLeg(LegPurpose.BackToStay, trip.StayName ?? "Konaklama", here, stay, leaveAt);
            return;
        }

        // Konaklamada.
        var local = KktcTime.ToLocal(arrivedAt);
        var today = DateOnly.FromDateTime(local.DateTime);

        // Akşamüstü dönenlerin bir kısmı akşam yemeğine (kasaba merkezi / liman) çıkar.
        if (local.Hour is >= 16 and < 21 && rng.Chance(0.45))
        {
            var town = ctx.NearbyTown(here);
            var dinnerAt = arrivedAt.AddMinutes(rng.Next(45, 120));
            if (dinnerAt + SimContext.EstimateTravel(here, town.Location) * 2 + PlannedDwell <= latestDepart)
            {
                trip.PlanLeg(LegPurpose.Excursion, $"Akşam yemeği · {town.Name}", here, town.Location, dinnerAt);
                return;
            }
        }

        // Gece dönen ertesi (aynı gün) sabah, öğleden önce hemen, öğleden sonra yarı yarıya ikinci gezi, yoksa yarın sabah.
        var departAt = local.Hour switch
        {
            < 7 => ctx.LocalToUtc(today, rng.Next(8, 11), rng.Next(0, 4) * 15),
            < 13 => arrivedAt.AddMinutes(rng.Next(30, 100)),
            < 19 when rng.Chance(0.55) => arrivedAt.AddMinutes(rng.Next(40, 120)),
            _ => ctx.LocalToUtc(today.AddDays(1), rng.Next(8, 11), rng.Next(0, 4) * 15),
        };

        var attraction = PickAttraction(ctx, trip, stay, 25_000);
        var roundTrip = SimContext.EstimateTravel(stay, attraction.Location) * 2 + PlannedDwell;
        if (departAt + roundTrip > latestDepart)
        {
            trip.PlanLeg(LegPurpose.ToReturnBranch, returnBranch.Name, here, returnBranch.Location, Max(arrivedAt.AddMinutes(15), latestDepart));
            return;
        }

        trip.LastAttraction = attraction.Name;
        trip.PlanLeg(LegPurpose.Excursion, attraction.Name, here, attraction.Location, departAt);
    }

    /// <summary>Son gidilen hariç bir gezi noktası; büyük ve <paramref name="from"/>'a yakın olan ağır basar.</summary>
    private static Place PickAttraction(SimContext ctx, Trip trip, GeoPoint from, double distanceScaleMeters) =>
        ctx.Rng.Pick(
            ctx.Places.Attractions.Where(a => a.Name != trip.LastAttraction).ToList(),
            a => a.Weight / (1 + GeoMath.DistanceMeters(from, a.Location) / distanceScaleMeters));

    private static DateTimeOffset LatestDeparture(Trip trip, GeoPoint here, GeoPoint returnLocation)
    {
        var returnAt = trip.ReturnAt ?? DateTimeOffset.MaxValue.AddYears(-1);
        var late = SimRandom.For(trip.ReservationId ?? trip.Id);
        var lateness = late.Chance(0.10) ? TimeSpan.FromMinutes(late.Next(60, 240)) : TimeSpan.Zero;
        return returnAt - SimContext.EstimateTravel(here, returnLocation) - ReturnBuffer + lateness;
    }

    private static DateTimeOffset Max(DateTimeOffset a, DateTimeOffset b) => a > b ? a : b;

    private static DateTimeOffset Min(DateTimeOffset a, DateTimeOffset b) => a < b ? a : b;
}
