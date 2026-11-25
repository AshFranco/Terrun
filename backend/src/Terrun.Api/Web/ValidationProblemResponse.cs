using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Terrun.Api.Web;

// LLD §3.4 and §7.3: every 400 VALIDATION_ERROR has "errors" keyed by the camelCase path of the field, as the app sends
// it ("type", "points[3].lat", "limit"). JSON that cannot be converted comes from the serializer keyed "$.type", with a message
// that names internal .NET types and an extra "the request field is required"; this rewrites both
internal static class ValidationProblemResponse
{
    public const string InvalidValueMessage = "The value is not valid.";
    private const string _bodyKey = "body";

    public static IActionResult Create(ActionContext context)
    {
        var parameterNames = context.ActionDescriptor.Parameters.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var normalized = new ModelStateDictionary();
        foreach (var (key, entry) in context.ModelState)
        {
            foreach (var error in entry?.Errors ?? [])
            {
                normalized.AddModelError(FieldOf(key, parameterNames), MessageOf(error));
            }
        }

        // The parameter itself fails only because its body could not be read: keep it only when it is the sole error
        if (normalized.Count > 1)
        {
            normalized.Remove(_bodyKey);
        }

        var problem = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>()
            .CreateValidationProblemDetails(context.HttpContext, normalized, StatusCodes.Status400BadRequest);
        return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
    }

    // Query parameters bind under their C# names ("Limit"); ModelState keys ignore case, so a validation error for "limit"
    // lands on that entry. Every segment is camelCased here, the same as the JSON the app sends
    private static string FieldOf(string key, HashSet<string> parameterNames)
    {
        var field = key.StartsWith("$.", StringComparison.Ordinal) ? key[2..] : key.TrimStart('$');
        return field.Length == 0 || parameterNames.Contains(field)
            ? _bodyKey
            : string.Join('.', field.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));
    }

    private static string MessageOf(ModelError error) =>
        error.Exception is not null || error.ErrorMessage.Contains("could not be converted", StringComparison.Ordinal)
            ? InvalidValueMessage
            : error.ErrorMessage;
}
