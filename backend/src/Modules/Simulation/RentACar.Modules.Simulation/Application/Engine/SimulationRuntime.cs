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

    /// <summary>
    /// Hızlı ileri sarma: saat durur, her karede <see cref="StepCatchUp"/> ile sabit adım ilerler. Saat duvar saatiyle
    /// aksaydı uzun süren bir kare (toplu rota isteği) saatlerce simüle zamanı atlardı; teslimatlar kaçar, müşteri "gelmedi" düşerdi.
    /// </summary>
    public void BeginCatchUp()
    {
        CatchingUp = true;
        clock.SetSpeed(0);
    }

    public void StepCatchUp(TimeSpan step) => clock.Reset(clock.UtcNow + step);

    /// <summary>Gerçek saate yetişildi: saat tam gerçek ana oturtulur ve normal hıza dönülür.</summary>
    public void EndCatchUp(DateTimeOffset realNow)
    {
        CatchingUp = false;
        clock.Reset(realNow);
        clock.SetSpeed(Running ? Speed : 0);
    }
}
