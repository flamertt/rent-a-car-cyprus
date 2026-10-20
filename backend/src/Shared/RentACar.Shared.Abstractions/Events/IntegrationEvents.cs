namespace RentACar.Shared.Abstractions.Events;

/// <summary>Modüller arası olay. Yayınlayan modülün Contracts projesinde tanımlanır.</summary>
public interface IIntegrationEvent
{
    DateTimeOffset OccurredAt { get; }
}

public interface IIntegrationEventHandler<in TEvent> where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken);
}

/// <summary>
/// Süreç içi olay veriyolu. İşleyiciler yayınlayanla aynı DI kapsamında, sırayla çağrılır;
/// ileride outbox + broker'a geçilirse yalnız bu arayüzün uygulaması değişir.
/// </summary>
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent;
}
