using Microsoft.Extensions.Options;
using RentACar.Modules.Simulation.Application.Engine;
using RentACar.Modules.Simulation.Infrastructure.Places;
using RentACar.Shared.Infrastructure.Time;

namespace RentACar.UnitTests.Simulation;

public sealed class EngineTests
{
    [Fact]
    public void Poisson_sampler_matches_mean()
    {
        var rng = new Random(42);
        var mean = Enumerable.Range(0, 20_000).Average(_ => rng.Poisson(0.7));
        Assert.InRange(mean, 0.66, 0.74);
    }

    [Fact]
    public void Embedded_places_are_inside_kktc()
    {
        var places = new EmbeddedPlacesCatalog();
        Assert.NotEmpty(places.Stays);
        Assert.NotEmpty(places.Attractions);
        Assert.All(places.Stays.Concat(places.Attractions), p => Assert.True(p.Location.IsInsideKktcBounds, p.Name));
    }

    [Fact]
    public void Clock_runs_at_speed_and_freezes_when_paused()
    {
        var start = new DateTimeOffset(2026, 7, 1, 9, 0, 0, TimeSpan.Zero);
        var clock = new SimulationClock(Options.Create(new ClockOptions { StartAt = start, Speed = 3600 }));
        Thread.Sleep(50);
        Assert.InRange((clock.UtcNow - start).TotalSeconds, 150, 3600); // ≥ 50 ms × 3600

        clock.SetSpeed(0);
        var frozen = clock.UtcNow;
        Thread.Sleep(30);
        Assert.Equal(frozen, clock.UtcNow);

        clock.Reset(start);
        Assert.Equal(start, clock.UtcNow);
    }
}
