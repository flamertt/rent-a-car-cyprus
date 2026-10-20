using System.Collections.Concurrent;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Shared.Infrastructure.Messaging;

/// <summary>
/// Hafif aracı (MediatR yerine): komut/sorgunun çalışma zamanı tipine göre DI'dan tek işleyiciyi çözer.
/// İşleyiciler modül içinde <c>internal</c> kalır; çağrı public arayüz üzerinden yapıldığı için erişim sorunu olmaz.
/// </summary>
internal sealed class Dispatcher(IServiceProvider services) : IDispatcher
{
    private static readonly ConcurrentDictionary<(Type Request, Type Result), object> Invokers = new();

    public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default) =>
        Invoker<TResult>(typeof(CommandInvoker<,>), command).InvokeAsync(services, command, cancellationToken);

    public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default) =>
        Invoker<TResult>(typeof(QueryInvoker<,>), query).InvokeAsync(services, query, cancellationToken);

    private static IRequestInvoker<TResult> Invoker<TResult>(Type openInvoker, object request) =>
        (IRequestInvoker<TResult>)Invokers.GetOrAdd(
            (request.GetType(), typeof(TResult)),
            key => Activator.CreateInstance(openInvoker.MakeGenericType(key.Request, key.Result))!);
}
