using RentACar.Modules.Simulation.Domain.Enums;
using RentACar.Shared.Abstractions.Geo;

namespace RentACar.Modules.Simulation.Domain.Entities;

/// <summary>
/// Bir aracın simüle edilen yolculuğu. Bacaklar tembel planlanır: yalnız mevcut/sonraki bacak tutulur,
/// varışta planlayıcı saat ve iade zamanına bakıp bir sonrakini seçer.
/// </summary>
internal sealed class Trip
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public TripKind Kind { get; set; }

    public TripState State { get; set; }

    public Guid VehicleId { get; set; }

    public string Plate { get; set; } = null!;

    public Guid? ReservationId { get; set; }

    public string? ReservationCode { get; set; }

    public string? CustomerName { get; set; }

    public Guid? TargetBranchId { get; set; }

    /// <summary>Kiralamada müşterinin dönmesi gereken an (planlayıcı buna göre iadeye yönelir).</summary>
    public DateTimeOffset? ReturnAt { get; set; }

    public string? StayName { get; set; }

    public double? StayLon { get; set; }

    public double? StayLat { get; set; }

    // ---- mevcut bacak ----
    public LegPurpose Purpose { get; set; }

    public string DestinationName { get; set; } = null!;

    public double FromLon { get; set; }

    public double FromLat { get; set; }

    public double ToLon { get; set; }

    public double ToLat { get; set; }

    public DateTimeOffset DepartAt { get; set; }

    public Guid? RouteId { get; set; }

    public DateTimeOffset? LegStartedAt { get; set; }

    public double LegDurationSeconds { get; set; }

    public double LegDistanceMeters { get; set; }

    public string? LastAttraction { get; set; }

    // ---- araç sayaçları (simülasyon tarafı) ----
    public double OdometerKm { get; set; }

    public double FuelLevel { get; set; }

    public double StartFuelLevel { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? FinishedAt { get; set; }

    public GeoPoint From => new(FromLon, FromLat);

    public GeoPoint To => new(ToLon, ToLat);

    public GeoPoint? Stay => StayLon is { } lon && StayLat is { } lat ? new GeoPoint(lon, lat) : null;

    public void PlanLeg(LegPurpose purpose, string destinationName, GeoPoint from, GeoPoint to, DateTimeOffset departAt)
    {
        Purpose = purpose;
        DestinationName = destinationName;
        FromLon = from.Lon;
        FromLat = from.Lat;
        ToLon = to.Lon;
        ToLat = to.Lat;
        DepartAt = departAt;
        RouteId = null;
        LegStartedAt = null;
        LegDurationSeconds = 0;
        LegDistanceMeters = 0;
        State = TripState.Waiting;
    }

    public void Finish(DateTimeOffset at)
    {
        State = TripState.Finished;
        FinishedAt = at;
    }
}
