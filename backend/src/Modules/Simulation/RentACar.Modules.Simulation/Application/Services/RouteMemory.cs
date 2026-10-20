using System.Collections.Concurrent;
using RentACar.Modules.Simulation.Domain.ValueObjects;

namespace RentACar.Modules.Simulation.Application.Services;

/// <summary>Süreç ömrü boyunca rota belleği (kimliğe ve uç noktalara göre); motor her karede buradan örnekler.</summary>
internal sealed class RouteMemory
{
    public ConcurrentDictionary<Guid, RoutePath> ById { get; } = new();

    public ConcurrentDictionary<string, Guid> ByKey { get; } = new();
}
