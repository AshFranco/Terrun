using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Terrun.Domain.Errors;

namespace Terrun.Api.Web;

// LLD §3.4: turns exceptions into ProblemDetails with a stable "code". Unexpected errors never expose their message
internal sealed class DomainExceptionHandler(IProblemDetailsService problemDetails, ILogger<DomainExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, code, detail) = Map(exception);
        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception");
        }

        httpContext.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails =
            {
                Status = status,
                Detail = detail,
                Extensions = { [ProblemDetailsSetup.CodeKey] = code },
            },
        });
    }

    private static (int Status, string Code, string? Detail) Map(Exception exception) => exception switch
    {
        InvalidRequestException e => (StatusCodes.Status400BadRequest, e.Code, e.Message),
        NotFoundException e => (StatusCodes.Status404NotFound, e.Code, e.Message),
        ConflictException e => (StatusCodes.Status409Conflict, e.Code, e.Message),
        ForbiddenException e => (StatusCodes.Status403Forbidden, e.Code, e.Message),
        UnauthorizedException e => (StatusCodes.Status401Unauthorized, e.Code, e.Message),
        BusinessRuleException e => (StatusCodes.Status422UnprocessableEntity, e.Code, e.Message),
        PayloadTooLargeException e => (StatusCodes.Status413PayloadTooLarge, e.Code, e.Message),
        DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, ErrorCodes.ConcurrencyConflict, null),
        BadHttpRequestException e when e.StatusCode == StatusCodes.Status413PayloadTooLarge =>
            (e.StatusCode, ErrorCodes.PayloadTooLarge, null),
        BadHttpRequestException e => (e.StatusCode, ErrorCodes.ValidationError, null),
        _ => (StatusCodes.Status500InternalServerError, ErrorCodes.InternalError, null),
    };
}
