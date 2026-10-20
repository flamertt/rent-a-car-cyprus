using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentACar.Modules.Simulation.Application.Abstractions;
using RentACar.Modules.Simulation.Application.Options;
using RentACar.Shared.Abstractions.Geo;

namespace RentACar.Modules.Simulation.Infrastructure.Routing;

/// <summary>
/// OSRM HTTP istemcisi (<c>/route/v1/driving</c>, segment süre/mesafe anotasyonlarıyla). Sunucu yoksa iki dakika
/// boyunca yeniden denenmez (devre kesici) ve <see cref="StraightLineRouter"/> yedeği döner; kamu demo sunucusu
/// için istek aralığı sınırlanabilir.
/// </summary>
internal sealed class OsrmRouteProvider(HttpClient http, IOptions<SimulationOptions> options, ILogger<OsrmRouteProvider> logger) : IRouteProvider
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static DateTimeOffset _lastRequest = DateTimeOffset.MinValue;
    private static DateTimeOffset _openUntil = DateTimeOffset.MinValue;

    public async Task<RouteResult> RouteAsync(GeoPoint from, GeoPoint to, CancellationToken cancellationToken) =>
        await TryOsrmAsync(from, to, cancellationToken) ?? StraightLineRouter.Route(from, to);

    private async Task<RouteResult?> TryOsrmAsync(GeoPoint from, GeoPoint to, CancellationToken ct)
    {
        var routing = options.Value.Routing;
        if (string.IsNullOrWhiteSpace(routing.OsrmUrl) || DateTimeOffset.UtcNow < _openUntil)
        {
            return null;
        }

        await Gate.WaitAsync(ct);
        try
        {
            var wait = _lastRequest.AddMilliseconds(routing.MinRequestIntervalMs) - DateTimeOffset.UtcNow;
            if (wait > TimeSpan.Zero)
            {
                await Task.Delay(wait, ct);
            }

            _lastRequest = DateTimeOffset.UtcNow;
            var url = string.Create(CultureInfo.InvariantCulture,
                $"{routing.OsrmUrl.TrimEnd('/')}/route/v1/driving/{from.Lon:F6},{from.Lat:F6};{to.Lon:F6},{to.Lat:F6}?overview=full&geometries=geojson&annotations=duration,distance&steps=false");
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(routing.TimeoutSeconds));
            using var response = await http.GetAsync(url, cts.Token);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("OSRM {Status}: {Url}", (int)response.StatusCode, url);
                return null;
            }

            await using var body = await response.Content.ReadAsStreamAsync(cts.Token);
            using var doc = await JsonDocument.ParseAsync(body, cancellationToken: cts.Token);
            return Parse(doc.RootElement);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !ct.IsCancellationRequested)
        {
            _openUntil = DateTimeOffset.UtcNow.AddMinutes(2);
            logger.LogWarning("OSRM erişilemiyor ({Message}); 2 dk boyunca düz çizgi yedeği kullanılacak", ex.Message);
            return null;
        }
        finally
        {
            Gate.Release();
        }
    }

    private static RouteResult? Parse(JsonElement root)
    {
        if (root.GetProperty("code").GetString() != "Ok" || root.GetProperty("routes").GetArrayLength() == 0)
        {
            return null;
        }

        var route = root.GetProperty("routes")[0];
        var points = route.GetProperty("geometry").GetProperty("coordinates").EnumerateArray()
            .Select(c => new GeoPoint(c[0].GetDouble(), c[1].GetDouble()))
            .ToArray();
        var annotation = route.GetProperty("legs")[0].GetProperty("annotation");
        var durations = annotation.GetProperty("duration").EnumerateArray().Select(d => d.GetDouble()).ToArray();
        var distances = annotation.GetProperty("distance").EnumerateArray().Select(d => d.GetDouble()).ToArray();
        return points.Length >= 2 && durations.Length == points.Length - 1 && distances.Length == points.Length - 1
            ? new RouteResult("osrm", points, durations, distances)
            : null;
    }
}
