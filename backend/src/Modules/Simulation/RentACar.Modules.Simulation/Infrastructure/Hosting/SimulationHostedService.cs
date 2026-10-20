using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentACar.Modules.Simulation.Application.Engine;
using RentACar.Modules.Simulation.Application.Options;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Modules.Simulation.Infrastructure.Hosting;

/// <summary>
/// Simülasyon döngüsü: her karede (varsayılan 500 ms gerçek zaman) yeni DI kapsamında bir <see cref="SimulationTick"/>.
/// Gerçek zamana kilitliyken saat geride kalırsa yetişme hızına geçer, yetişince 1×'e döner.
/// </summary>
internal sealed class SimulationHostedService(
    IServiceScopeFactory scopes,
    SimulationRuntime runtime,
    IClock clock,
    IOptions<SimulationOptions> options,
    ILogger<SimulationHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan MaxLag = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(Math.Max(100, options.Value.TickMilliseconds)));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            if (!runtime.Running)
            {
                continue;
            }

            FollowRealTime();
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<SimulationTick>().RunAsync(stoppingToken);
                runtime.LastError = null;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                runtime.LastError = ex.Message;
                logger.LogError(ex, "Simülasyon karesi başarısız");
            }
        }
    }

    private void FollowRealTime()
    {
        if (!options.Value.FollowRealTime)
        {
            return;
        }

        var realNow = TimeProvider.System.GetUtcNow();
        var lag = realNow - clock.UtcNow;
        if (lag > MaxLag && !runtime.CatchingUp)
        {
            logger.LogInformation("Saat gerçek zamanın {Lag} gerisinde; hızlı ileri sarılıyor", lag);
            runtime.BeginCatchUp(options.Value.CatchUpSpeed);
        }
        else if (lag <= TimeSpan.Zero && runtime.CatchingUp)
        {
            runtime.EndCatchUp(realNow);
            logger.LogInformation("Gerçek zamana yetişildi");
        }
    }
}
