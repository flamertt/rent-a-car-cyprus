namespace RentACar.Shared.Abstractions.Events;

/// <summary>Modüller arası olay. Yayınlayan modülün Contracts projesinde tanımlanır.</summary>
public interface IIntegrationEvent
{
    DateTimeOffset OccurredAt { get; }
}
