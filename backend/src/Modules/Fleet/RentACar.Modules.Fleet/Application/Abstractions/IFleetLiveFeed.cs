using RentACar.Modules.Fleet.Application.Dtos;

namespace RentACar.Modules.Fleet.Application.Abstractions;

/// <summary>Filonun bellekteki anlık görüntüsü; SSE akışı yalnız son gönderimden beri değişenleri iter.</summary>
internal interface IFleetLiveFeed
{
    long Version { get; }

    void Upsert(LiveVehicleDto vehicle);

    void Remove(Guid vehicleId);

    bool TryGet(Guid vehicleId, out LiveVehicleDto vehicle);

    IReadOnlyList<LiveVehicleDto> ChangedSince(long sinceVersion, out long currentVersion);
}
