using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentACar.Modules.Simulation.Application.Engine;
using RentACar.Modules.Simulation.Application.Options;
using RentACar.Modules.Simulation.Domain.Entities;
using RentACar.Shared.Abstractions.Modules;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Simulation.Infrastructure.Persistence;

/// <summary>
/// Migration + saati kaldığı yerden sürdürme. Gerçek zamana kilitliyken ilk açılış <c>WarmUpDays</c> gün geriden
/// başlar; aradaki süreyi motor hızlı ileri sararak doldurur (panel geçmişi olan bir operasyonla açılır).
/// </summary>
internal sealed class SimulationInitializer(
    SimulationDbContext db,
    SimulationRuntime runtime,
    IClock clock,
    IOptions<SimulationOptions> options,
    ILogger<SimulationInitializer> logger) : IModuleInitializer
{
    public int Order => 60;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await db.Database.MigrateAsync(cancellationToken);
        var o = options.Value;
        var realNow = TimeProvider.System.GetUtcNow();
        var state = await db.State.SingleOrDefaultAsync(cancellationToken);
        if (state is null)
        {
            var start = o.FollowRealTime ? realNow.AddDays(-o.WarmUpDays) : clock.UtcNow;
            state = new SimState
            {
                Running = o.AutoStart,
                Speed = o.FollowRealTime ? 1 : Math.Max(1, clock.Speed),
                SimNow = start,
                LastDemandAt = start,
            };
            db.State.Add(state);
            await db.SaveChangesAsync(cancellationToken);
        }

        if (o.FollowRealTime && state.SimNow > realNow.AddMinutes(1))
        {
            logger.LogWarning("Kayıtlı simülasyon saati ({SimNow}) gerçek saatin ilerisinde; saat geri alınamaz, veriler ileri tarihli görünecek", state.SimNow);
        }

        clock.Reset(state.SimNow);
        runtime.Apply(state.Running, o.FollowRealTime ? 1 : state.Speed);
    }
}
