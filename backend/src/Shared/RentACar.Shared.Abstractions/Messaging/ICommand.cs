namespace RentACar.Shared.Abstractions.Messaging;

/// <summary>Durum değiştiren kullanım senaryosu (CQRS yazma tarafı).</summary>
public interface ICommand<TResult>;
