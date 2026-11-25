namespace Terrun.Api.Web;

// Declares an error that an action can answer, with the codes of the LLD §3.4 catalog it uses. The errors every
// endpoint shares (validation, size, rate limit, token, internal) are added by OpenApiErrorResponses on their own
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class ApiErrorAttribute(int statusCode, params string[] codes) : Attribute
{
    public int StatusCode { get; } = statusCode;

    public IReadOnlyList<string> Codes { get; } = codes;
}
