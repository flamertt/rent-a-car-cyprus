using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RentACar.Shared.Abstractions.Exceptions;

namespace RentACar.Shared.Infrastructure.Errors;

/// <summary>Domain istisnalarını RFC 9457 ProblemDetails'e çevirir; <c>code</c> uzantısı istemci içindir.</summary>
internal sealed class AppExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, code) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "not_found"),
            ConflictException c => (StatusCodes.Status409Conflict, c.Code),
            DomainException d => (StatusCodes.Status422UnprocessableEntity, d.Code),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "bad_request"),
            _ => (0, string.Empty),
        };
        if (status == 0)
        {
            return false;
        }

        httpContext.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = exception.Message,
                Extensions = { ["code"] = code },
            },
        });
    }
}
