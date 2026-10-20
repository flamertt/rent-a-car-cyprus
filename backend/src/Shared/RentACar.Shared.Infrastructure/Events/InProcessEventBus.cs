using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RentACar.Shared.Abstractions.Events;

namespace RentACar.Shared.Infrastructure.Events;

internal sealed class InProcessEventBus(IServiceProvider services, ILogger<InProcessEventBus> logger) : IEventBus
{
    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        var handlers = services.GetServices<IIntegrationEventHandler<TEvent>>().ToList();
        logger.LogDebug("{Event} → {Count} işleyici", typeof(TEvent).Name, handlers.Count);
        foreach (var handler in handlers)
        {
            await handler.HandleAsync(integrationEvent, cancellationToken);
        }
    }
}
