using System.Globalization;
using Microsoft.EntityFrameworkCore;
using RentACar.Modules.Simulation.Application.Abstractions;
using RentACar.Modules.Simulation.Domain.Entities;
using RentACar.Modules.Simulation.Domain.ValueObjects;
using RentACar.Shared.Abstractions.Geo;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Simulation.Application.Services;

/// <summary>
/// Rota çözümü: bellek → veritabanı önbelleği → yönlendirme servisi. OSRM rotaları kalıcı önbelleğe yazılır;
/// düz çizgi yedekleri yalnız bellekte tutulur ki OSRM sonradan açılınca gerçek rota alınsın.
/// </summary>
internal sealed class RouteService(ISimulationDbContext db, IRouteProvider router, RouteMemory memory, IClock clock)
{
    public async Task<RoutePath> GetAsync(GeoPoint from, GeoPoint to, CancellationToken ct)
    {
        var key = Key(from, to);
        if (memory.ByKey.TryGetValue(key, out var cachedId) && memory.ById.TryGetValue(cachedId, out var cached))
        {
            return cached;
        }

        var stored = await db.Routes.AsNoTracking().FirstOrDefaultAsync(r => r.Key == key, ct);
        if (stored is not null)
        {
            return Remember(key, ToPath(stored));
        }

        var result = await router.RouteAsync(from, to, ct);
        var path = new RoutePath(Guid.CreateVersion7(), result.Provider, result.Points, result.SegmentDurations, result.SegmentDistances);
        if (result.Provider == "osrm")
        {
            var record = new RouteRecord
            {
                Id = path.Id,
                Key = key,
                Provider = result.Provider,
                Path = result.Points.ToLineString(),
                SegmentDurations = result.SegmentDurations,
                SegmentDistances = result.SegmentDistances,
                DistanceMeters = path.DistanceMeters,
                DurationSeconds = path.DurationSeconds,
                CreatedAt = clock.UtcNow,
            };
            db.Routes.Add(record);
            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                // Aynı anahtar eşzamanlı yazıldı: yalnız bu kaydı bırak (karedeki diğer değişiklikler korunur).
                db.Routes.Remove(record);
            }
        }

        return Remember(key, path);
    }

    /// <summary>Yeniden başlatmadan sonra yolculuğun kayıtlı rotası; yoksa uçlardan yeniden çözülür.</summary>
    public async Task<RoutePath> GetByIdOrResolveAsync(Guid? id, GeoPoint from, GeoPoint to, CancellationToken ct)
    {
        if (id is { } routeId)
        {
            if (memory.ById.TryGetValue(routeId, out var cached))
            {
                return cached;
            }

            var stored = await db.Routes.AsNoTracking().FirstOrDefaultAsync(r => r.Id == routeId, ct);
            if (stored is not null)
            {
                return Remember(stored.Key, ToPath(stored));
            }
        }

        return await GetAsync(from, to, ct);
    }

    /// <summary>Yedek (düz çizgi) rota yalnız kimliğiyle tutulur: aynı uçlar istenince OSRM yeniden denenir.</summary>
    private RoutePath Remember(string key, RoutePath path)
    {
        memory.ById[path.Id] = path;
        if (path.Provider == "osrm")
        {
            memory.ByKey[key] = path.Id;
        }

        return path;
    }

    private static RoutePath ToPath(RouteRecord r) => new(
        r.Id, r.Provider, r.Path.Coordinates.Select(c => new GeoPoint(c.X, c.Y)).ToArray(), r.SegmentDurations, r.SegmentDistances);

    private static string Key(GeoPoint from, GeoPoint to) =>
        string.Create(CultureInfo.InvariantCulture, $"{from.Lon:F4},{from.Lat:F4}>{to.Lon:F4},{to.Lat:F4}");
}
