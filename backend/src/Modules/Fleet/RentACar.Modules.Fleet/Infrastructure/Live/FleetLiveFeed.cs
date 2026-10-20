using System.Collections.Concurrent;
using RentACar.Modules.Fleet.Application.Abstractions;
using RentACar.Modules.Fleet.Application.Dtos;

namespace RentACar.Modules.Fleet.Infrastructure.Live;

/// <summary>
/// Süreç içi, sürüm numaralı anlık görüntü. Tek süreçli monolit için yeterli; yatay ölçeklemede
/// Redis pub/sub vb. ile değiştirilir (arayüz aynı kalır).
/// </summary>
internal sealed class FleetLiveFeed : IFleetLiveFeed
{
    private readonly ConcurrentDictionary<Guid, (LiveVehicleDto Vehicle, long Version)> _vehicles = new();
    private long _version;

    public long Version => Interlocked.Read(ref _version);

    public void Upsert(LiveVehicleDto vehicle)
    {
        var version = Interlocked.Increment(ref _version);
        _vehicles[vehicle.Id] = (vehicle, version);
    }

    public void Remove(Guid vehicleId) => _vehicles.TryRemove(vehicleId, out _);

    public bool TryGet(Guid vehicleId, out LiveVehicleDto vehicle)
    {
        var found = _vehicles.TryGetValue(vehicleId, out var entry);
        vehicle = entry.Vehicle;
        return found;
    }

    public IReadOnlyList<LiveVehicleDto> ChangedSince(long sinceVersion, out long currentVersion)
    {
        currentVersion = Version;
        return _vehicles.Values.Where(e => e.Version > sinceVersion).Select(e => e.Vehicle).ToList();
    }
}
