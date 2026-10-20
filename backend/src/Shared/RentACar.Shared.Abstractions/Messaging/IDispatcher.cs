namespace RentACar.Shared.Abstractions.Messaging;

/// <summary>Komut/sorguyu DI'daki tek işleyicisine yönlendirir; uç noktalar ve modül API'leri bunu kullanır.</summary>
public interface IDispatcher
{
    Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);

    Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}
