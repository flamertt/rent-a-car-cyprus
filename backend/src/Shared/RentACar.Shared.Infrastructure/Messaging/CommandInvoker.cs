using Microsoft.Extensions.DependencyInjection;
using RentACar.Shared.Abstractions.Messaging;

namespace RentACar.Shared.Infrastructure.Messaging;

internal sealed class CommandInvoker<TCommand, TResult> : IRequestInvoker<TResult>
    where TCommand : ICommand<TResult>
{
    public Task<TResult> InvokeAsync(IServiceProvider services, object request, CancellationToken cancellationToken) =>
        services.GetRequiredService<ICommandHandler<TCommand, TResult>>().HandleAsync((TCommand)request, cancellationToken);
}
