namespace RentACar.Shared.Infrastructure.Messaging;

/// <summary>Kapalı generic işleyici çağırıcısı; yansıma yalnız ilk çağrıda, sonra önbellekten.</summary>
internal interface IRequestInvoker<TResult>
{
    Task<TResult> InvokeAsync(IServiceProvider services, object request, CancellationToken cancellationToken);
}
