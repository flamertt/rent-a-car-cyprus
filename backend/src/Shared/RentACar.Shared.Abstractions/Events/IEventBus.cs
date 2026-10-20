namespace RentACar.Shared.Abstractions.Events;

/// <summary>
/// Süreç içi olay veriyolu. İşleyiciler yayınlayanla aynı DI kapsamında, sırayla çağrılır;
/// ileride outbox + broker'a geçilirse yalnız bu arayüzün uygulaması değişir.
/// </summary>
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent;
}
