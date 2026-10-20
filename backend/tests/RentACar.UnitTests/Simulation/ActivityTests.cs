using RentACar.Modules.Branches.Contracts.Dtos;
using RentACar.Modules.Branches.Contracts.Enums;
using RentACar.Modules.Simulation.Application.Engine;
using RentACar.Modules.Simulation.Domain.Entities;
using RentACar.Modules.Simulation.Domain.Enums;
using RentACar.Modules.Simulation.Infrastructure.Places;
using RentACar.Shared.Abstractions.Geo;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.UnitTests.Simulation;

/// <summary>Gezi planı ve asgari hareket kuralı (haritada sürekli yolda araç).</summary>
public sealed class ActivityTests
{
    private static readonly EmbeddedPlacesCatalog Places = new();

    private static readonly BranchDto Kyrenia = new(
        Guid.NewGuid(), "GRN-OFC", "Girne Ofis", BranchType.Office, default, "Girne", new GeoPoint(33.3195, 35.3378),
        null, new TimeOnly(8, 0), new TimeOnly(20, 0), true, 0);

    [Fact]
    public void Customer_back_at_stay_after_midnight_waits_for_the_morning()
    {
        for (var seed = 0; seed < 50; seed++)
        {
            var arrivedAt = Local(2026, 7, 10, 2, 30);
            var ctx = Context(arrivedAt, seed);
            var trip = RentalAtStay(arrivedAt, returnAt: arrivedAt.AddDays(4));
            trip.Purpose = LegPurpose.BackToStay;

            ItineraryPlanner.PlanNext(trip, ctx, arrivedAt);

            Assert.Equal(LegPurpose.Excursion, trip.Purpose);
            Assert.InRange(KktcTime.ToLocal(trip.DepartAt).Hour, 8, 10);
            Assert.Equal(KktcTime.LocalDate(arrivedAt), KktcTime.LocalDate(trip.DepartAt));
        }
    }

    [Fact]
    public void Imminent_departures_count_as_moving()
    {
        var now = Local(2026, 7, 10, 14, 0);
        var driving = RentalAtStay(now, now.AddDays(3));
        driving.State = TripState.Driving;
        var soon = RentalAtStay(now, now.AddDays(3));
        soon.DepartAt = now.AddMinutes(5);
        var later = RentalAtStay(now, now.AddDays(3));
        later.DepartAt = now.AddHours(3);
        var done = RentalAtStay(now, now.AddDays(3));
        done.Finish(now);

        Assert.Equal(2, ActivityFloor.CountMoving([driving, soon, later, done], now));
    }

    [Fact]
    public void Daytime_floor_only_brings_forward_customers_who_were_leaving_soon_anyway()
    {
        var now = Local(2026, 7, 10, 15, 0);
        var ctx = Context(now, seed: 7);
        var soon = Enumerable.Range(0, 4).Select(_ => Parked(now, now.AddDays(3), now.AddMinutes(90))).ToList();
        var tomorrow = Enumerable.Range(0, 6).Select(_ => Parked(now, now.AddDays(3), now.AddHours(18))).ToList();
        var returning = Parked(now, now.AddHours(3), now.AddHours(1));
        var trips = soon.Concat(tomorrow).Append(returning).ToList();

        var started = ActivityFloor.SendCustomersOut(ctx, trips, count: 10);

        Assert.Equal(4, started);
        Assert.All(soon, t => Assert.True(t.DepartAt <= now.AddMinutes(13)));
        Assert.All(tomorrow, t => Assert.Equal(now.AddHours(18), t.DepartAt)); // otelde park hâlinde kalır
        Assert.Equal(now.AddHours(1), returning.DepartAt);
    }

    [Fact]
    public void Evening_floor_sends_customers_to_town_and_planner_brings_them_back()
    {
        var now = Local(2026, 7, 10, 22, 0);
        var ctx = Context(now, seed: 3);
        var trip = Parked(now, now.AddDays(3), now.AddHours(11));

        Assert.Equal(1, ActivityFloor.SendCustomersOut(ctx, [trip], count: 1));
        Assert.StartsWith("Gece eğlencesi", trip.DestinationName);
        Assert.Equal(LegPurpose.Excursion, trip.Purpose);
        Assert.True(trip.DepartAt <= now.AddMinutes(13));

        var arrivedAt = now.AddMinutes(40);
        ItineraryPlanner.PlanNext(trip, Context(arrivedAt, seed: 3), arrivedAt);
        Assert.Equal(LegPurpose.BackToStay, trip.Purpose);
    }

    [Fact]
    public void Customers_are_left_asleep_after_one_am()
    {
        var now = Local(2026, 7, 10, 3, 0);
        var trips = Enumerable.Range(0, 5).Select(_ => Parked(now, now.AddDays(3), now.AddHours(6))).ToList();
        Assert.Equal(0, ActivityFloor.SendCustomersOut(Context(now, seed: 1), trips, count: 5));
    }

    [Fact]
    public void Scattered_points_stay_within_radius_and_spread_out()
    {
        var center = Places.Stays[0].Location;
        var rng = new Random(11);
        var points = Enumerable.Range(0, 500).Select(_ => SimContext.Scatter(center, 1_400, rng)).ToList();
        Assert.All(points, p => Assert.InRange(GeoMath.DistanceMeters(center, p), 0, 1_401));
        Assert.True(points.Average(p => GeoMath.DistanceMeters(center, p)) > 700); // düzgün disk: ortalama ≈ 2/3 r
    }

    private static SimContext Context(DateTimeOffset now, int seed) => new(now, [], [Kyrenia], Places, new Random(seed));

    /// <summary>Konaklamaya yeni varmış müşteri (planlayıcı buradan bir sonraki hareketi seçer).</summary>
    private static Trip RentalAtStay(DateTimeOffset now, DateTimeOffset returnAt)
    {
        var stay = Places.Stays[0];
        var trip = new Trip
        {
            Kind = TripKind.Rental, VehicleId = Guid.NewGuid(), Plate = "ZAB 123", ReservationId = Guid.NewGuid(),
            TargetBranchId = Kyrenia.Id, ReturnAt = returnAt, StayName = stay.Name, StayLon = stay.Location.Lon, StayLat = stay.Location.Lat,
        };
        trip.PlanLeg(LegPurpose.ToStay, stay.Name, Kyrenia.Location, stay.Location, now.AddHours(-1));
        trip.ToLon = stay.Location.Lon;
        trip.ToLat = stay.Location.Lat;
        return trip;
    }

    /// <summary>Konaklamada park etmiş, bir sonraki gezisi <paramref name="departAt"/>'ta olan müşteri.</summary>
    private static Trip Parked(DateTimeOffset now, DateTimeOffset returnAt, DateTimeOffset departAt)
    {
        var trip = RentalAtStay(now, returnAt);
        var attraction = Places.Attractions[0];
        trip.PlanLeg(LegPurpose.Excursion, attraction.Name, trip.Stay!.Value, attraction.Location, departAt);
        return trip;
    }

    private static DateTimeOffset Local(int y, int m, int d, int h, int min) =>
        new(TimeZoneInfo.ConvertTimeToUtc(new DateTime(y, m, d, h, min, 0, DateTimeKind.Unspecified), KktcTime.Zone), TimeSpan.Zero);
}
