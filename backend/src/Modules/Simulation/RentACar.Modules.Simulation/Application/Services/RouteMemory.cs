using System.Collections.Concurrent;
using RentACar.Modules.Simulation.Domain.ValueObjects;

namespace RentACar.Modules.Simulation.Application.Services;

/// <summary>Süreç ömrü boyunca rota belleği (kimliğe ve uç noktalara göre); motor her karede buradan örnekler.</summary>
internal sealed class RouteMemory
{
    public ConcurrentDictionary<Guid, RoutePath> ById { get; } = new();

    public ConcurrentDictionary<string, Guid> ByKey { get; } = new();

    /// <summary>Bellek <paramref name="max"/> rotayı aşınca kullanılmayanları atar (gerekirse veritabanından yeniden okunur).</summary>
    public void Trim(IReadOnlySet<Guid> inUse, int max)
    {
        if (ById.Count <= max)
        {
            return;
        }

        foreach (var id in ById.Keys.Where(id => !inUse.Contains(id)))
        {
            ById.TryRemove(id, out _);
        }

        foreach (var (key, id) in ByKey.Where(kv => !ById.ContainsKey(kv.Value)))
        {
            ByKey.TryRemove(key, out _);
        }
    }
}
