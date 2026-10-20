using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Simulation.Application.Engine;

/// <summary>
/// Çalışma durumu (tekil). Duraklatma = saati durdurmak: rezervasyon zamanları da donar, tutarlılık korunur.
/// Motor tek iş parçacığında döndüğü için <see cref="Random"/> paylaşılabilir.
/// </summary>
internal sealed class SimulationRuntime(IClock clock)
{
    public bool Running { get; private set; }

    public double Speed { get; private set; } = 1;

    /// <summary>Gerçek saate yetişmek için hızlı ileri sarılıyor.</summary>
    public bool CatchingUp { get; private set; }

    public string? LastError { get; set; }

    public long Tick { get; set; }

    public Random Random { get; } = new();

    public void Apply(bool running, double speed)
    {
        Running = running;
        Speed = Math.Clamp(speed, 0.1, 3600);
        CatchingUp = false;
        clock.SetSpeed(Running ? Speed : 0);
    }

    public void BeginCatchUp(double speed)
    {
        CatchingUp = true;
        clock.SetSpeed(Running ? Math.Clamp(speed, 1, 3600) : 0);
    }

    /// <summary>Gerçek saate yetişildi: saat tam gerçek ana oturtulur ve normal hıza dönülür.</summary>
    public void EndCatchUp(DateTimeOffset realNow)
    {
        CatchingUp = false;
        clock.Reset(realNow);
        clock.SetSpeed(Running ? Speed : 0);
    }
}
