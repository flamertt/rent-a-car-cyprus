using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RentACar.Modules.Reservations.Application.Commands.MarkNoShows;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Modules.Reservations.Infrastructure.BackgroundJobs;

/// <summary>Gelmeyen müşterilerin rezervasyonlarını kapatır; aracı takvimde serbest bırakır (30 sn'de bir).</summary>
internal sealed class NoShowSweeper(IServiceScopeFactory scopes, ILogger<NoShowSweeper> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var count = await scope.ServiceProvider.GetRequiredService<IDispatcher>().SendAsync(new MarkNoShowsCommand(), stoppingToken);
                if (count > 0)
                {
                    logger.LogInformation("{Count} rezervasyon 'gelmedi' olarak kapatıldı", count);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Gelmedi taraması başarısız");
            }
        }
    }
}
