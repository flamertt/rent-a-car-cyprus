using RentACar.Modules.Reservations.Contracts.Enums;
using RentACar.Modules.Reservations.Domain.Entities;
using RentACar.Modules.Reservations.Domain.Rules;
using RentACar.Shared.Abstractions.Exceptions;

namespace RentACar.UnitTests.Reservations;

public sealed class ReservationTests
{
    private static readonly DateTimeOffset PickupAt = new(2026, 11, 10, 8, 0, 0, TimeSpan.Zero);
    private static readonly ReservationRules Rules = new();

    [Fact]
    public void Blocks_vehicle_until_return_plus_turnaround()
    {
        var r = New(days: 3);
        Assert.Equal(r.ReturnAt + Rules.Turnaround, r.BlockedUntil);
        Assert.Equal(ReservationStatus.Confirmed, r.Status);
        Assert.Matches("^KR-[A-Z2-9]{6}$", r.Code);
    }

    [Fact]
    public void Pickup_earlier_than_allowed_window_is_rejected()
    {
        var r = New(days: 3);
        var ex = Assert.Throws<DomainException>(() => r.PickUp(PickupAt.AddHours(-3), 10_000, 1, Rules));
        Assert.Equal("reservation.too_early", ex.Code);

        r.PickUp(PickupAt.AddHours(-1), 10_000, 1, Rules);
        Assert.Equal(ReservationStatus.Active, r.Status);
    }

    [Fact]
    public void On_time_full_tank_return_adds_nothing()
    {
        var r = New(days: 3);
        r.PickUp(PickupAt, 10_000, 1, Rules);
        r.Return(r.ReturnAt.AddMinutes(30), Guid.NewGuid(), 10_350, 1, Rules);

        Assert.Equal(ReservationStatus.Completed, r.Status);
        Assert.Equal(3_000, r.Total);
    }

    [Fact]
    public void Late_return_and_missing_fuel_are_charged()
    {
        var r = New(days: 3);
        r.PickUp(PickupAt, 10_000, 0.9, Rules);
        r.Return(r.ReturnAt.AddHours(5), Guid.NewGuid(), 10_500, 0.4, Rules);

        var late = r.Lines.Single(l => l.Code == "LATE_RETURN");
        Assert.Equal(1, late.Quantity);
        Assert.Equal(1_000, late.Amount); // RENTAL satırının günlük birim fiyatı
        Assert.Equal(2_000, r.Lines.Single(l => l.Code == "REFUEL").Amount); // %50 × 4.000
        Assert.Equal(3_000 + 1_000 + 2_000, r.Total);
    }

    [Fact]
    public void Odometer_cannot_go_backwards()
    {
        var r = New(days: 1);
        r.PickUp(PickupAt, 10_000, 1, Rules);
        Assert.Throws<DomainException>(() => r.Return(r.ReturnAt, Guid.NewGuid(), 9_999, 1, Rules));
    }

    [Fact]
    public void Status_machine_rejects_invalid_transitions()
    {
        var r = New(days: 2);
        r.PickUp(PickupAt, 1, 1, Rules);
        Assert.Throws<ConflictException>(() => r.Cancel(PickupAt));
        Assert.Throws<ConflictException>(() => r.PickUp(PickupAt, 1, 1, Rules));

        var other = New(days: 2);
        other.Cancel(PickupAt.AddDays(-1));
        Assert.Throws<ConflictException>(() => other.MarkNoShow(PickupAt.AddDays(1)));
    }

    private static Reservation New(int days) => Reservation.Create(
        BookingChannel.Simulation,
        (Guid.NewGuid(), "Test Kişi", "test@example.com"),
        (Guid.NewGuid(), "ECAR"),
        (Guid.NewGuid(), "ZAK 482"),
        Guid.NewGuid(),
        Guid.NewGuid(),
        PickupAt,
        PickupAt.AddDays(days),
        Rules.Turnaround,
        days,
        days * 1_000,
        "TRY",
        [new PriceLine { Code = "RENTAL", Description = "Kiralama", Quantity = days, UnitPrice = 1_000, Amount = days * 1_000 }],
        PickupAt.AddDays(-7));
}
