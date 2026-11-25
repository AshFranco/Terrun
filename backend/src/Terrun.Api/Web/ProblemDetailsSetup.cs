using System.Diagnostics;
using Terrun.Domain.Errors;

namespace Terrun.Api.Web;

internal static class ProblemDetailsSetup
{
    public const string CodeKey = "code";
    public const string TraceIdKey = "traceId";
    public const string ErrorsKey = "errors";

    // Every error response carries a code from the LLD §3.4 catalog and the traceId that support uses to find the log.
    // It is the same TraceId the JSON log writes in its scope, so one search finds the request
    public static void Customize(ProblemDetailsContext context)
    {
        var extensions = context.ProblemDetails.Extensions;
        extensions[TraceIdKey] = Activity.Current?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier;
        extensions.TryAdd(CodeKey, DefaultCode(context.ProblemDetails.Status));
    }

    private static string DefaultCode(int? status) => status switch
    {
        StatusCodes.Status400BadRequest or StatusCodes.Status415UnsupportedMediaType => ErrorCodes.ValidationError,
        StatusCodes.Status401Unauthorized => ErrorCodes.InvalidToken,
        StatusCodes.Status403Forbidden => ErrorCodes.Forbidden,
        StatusCodes.Status404NotFound or StatusCodes.Status405MethodNotAllowed => ErrorCodes.NotFound,
        StatusCodes.Status409Conflict => ErrorCodes.ConcurrencyConflict,
        StatusCodes.Status413PayloadTooLarge => ErrorCodes.PayloadTooLarge,
        StatusCodes.Status422UnprocessableEntity => ErrorCodes.BusinessRuleViolation,
        StatusCodes.Status429TooManyRequests => ErrorCodes.RateLimited,
        _ => ErrorCodes.InternalError,
    };
}
