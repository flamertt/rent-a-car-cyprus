using Microsoft.Extensions.DependencyInjection;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Shared.Infrastructure.Messaging;

internal sealed class QueryInvoker<TQuery, TResult> : IRequestInvoker<TResult>
    where TQuery : IQuery<TResult>
{
    public Task<TResult> InvokeAsync(IServiceProvider services, object request, CancellationToken cancellationToken) =>
        services.GetRequiredService<IQueryHandler<TQuery, TResult>>().HandleAsync((TQuery)request, cancellationToken);
}
