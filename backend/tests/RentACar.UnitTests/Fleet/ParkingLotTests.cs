using RentACar.Modules.Fleet.Domain.Services;
using RentACar.Shared.Abstractions.Geo;

namespace RentACar.UnitTests.Fleet;

public sealed class ParkingLotTests
{
    private static readonly GeoPoint Office = new(33.3195, 35.3378);

    [Fact]
    public void First_free_fills_gaps_before_growing()
    {
        Assert.Equal(0, ParkingLot.FirstFree([]));
        Assert.Equal(2, ParkingLot.FirstFree([0, 1, 3]));
        Assert.Equal(4, ParkingLot.FirstFree([3, 1, 0, 2]));
    }

    [Fact]
    public void Every_slot_of_a_large_lot_has_its_own_spot()
    {
        var spots = Enumerable.Range(0, 300).Select(s => ParkingLot.SpotFor(Office, s)).ToList();
        Assert.Equal(spots.Count, spots.Distinct().Count());
        var nearest = spots.Skip(1).Min(p => GeoMath.DistanceMeters(spots[0], p));
        Assert.InRange(nearest, 2.5, 6); // araç aralığı ~2,9 m
    }
}
