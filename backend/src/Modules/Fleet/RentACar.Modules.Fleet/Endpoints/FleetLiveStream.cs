using System.Runtime.CompilerServices;
using RentACar.Modules.Fleet.Application.Abstractions;
using RentACar.Modules.Fleet.Application.Dtos;

namespace RentACar.Modules.Fleet.Endpoints;

/// <summary>SSE kaynağı: ilk mesaj tam görüntü, sonrakiler yalnız değişenler (yarım saniyede bir).</summary>
internal static class FleetLiveStream
{
    public static async IAsyncEnumerable<IReadOnlyList<LiveVehicleDto>> ReadAsync(
        IFleetLiveFeed live, [EnumeratorCancellation] CancellationToken ct)
    {
        long version = 0;
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));
        do
        {
            var changed = live.ChangedSince(version, out version);
            if (changed.Count > 0)
            {
                yield return changed;
            }
        }
        while (await timer.WaitForNextTickAsync(ct));
    }
}
