using System.Globalization;
using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Terrun.Domain.Errors;

namespace Terrun.Api.Web;

// LLD §3.1 and §3.4 (CTZ-106): the OpenAPI declares every error an operation can answer, so the app types (CTZ-19) carry
// the shape of the error and the codes to translate instead of writing them by hand
internal static class OpenApiErrorResponses
{
    public const string SchemaName = "ApiProblem";
    public const string ProblemContentType = "application/problem+json";

    // The order the statuses appear in the document
    private static readonly int[] _statusOrder = [400, 401, 403, 404, 409, 413, 415, 422, 429, 500];

    public static Task AddProblemSchemaAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();
        document.Components.Schemas[SchemaName] = BuildProblemSchema();
        return Task.CompletedTask;
    }

    public static Task AddErrorResponsesAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor is not ControllerActionDescriptor action)
        {
            return Task.CompletedTask;
        }

        operation.Responses ??= [];
        foreach (var (status, codes) in ErrorsOf(action, context.Description.ParameterDescriptions))
        {
            operation.Responses[status.ToString(CultureInfo.InvariantCulture)] = new OpenApiResponse
            {
                Description = $"Error codes: {string.Join(", ", codes)}.",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    [ProblemContentType] = new() { Schema = new OpenApiSchemaReference(SchemaName, context.Document) },
                },
            };
        }

        return Task.CompletedTask;
    }

    // Shared errors first, then the ones the action declares with [ApiError]
    private static IEnumerable<(int Status, SortedSet<string> Codes)> ErrorsOf(
        ControllerActionDescriptor action,
        IList<Microsoft.AspNetCore.Mvc.ApiExplorer.ApiParameterDescription> parameters)
    {
        var errors = new Dictionary<int, SortedSet<string>>();
        void Add(int status, IEnumerable<string> codes)
        {
            if (!errors.TryGetValue(status, out var set))
            {
                errors[status] = set = new SortedSet<string>(StringComparer.Ordinal);
            }

            set.UnionWith(codes);
        }

        Add(StatusCodes.Status400BadRequest, [ErrorCodes.ValidationError]);
        Add(StatusCodes.Status429TooManyRequests, [ErrorCodes.RateLimited]);
        Add(StatusCodes.Status500InternalServerError, [ErrorCodes.InternalError]);

        // LLD §7.3: every body goes through the size limit and must be JSON
        if (parameters.Any(p => p.Source == BindingSource.Body))
        {
            Add(StatusCodes.Status413PayloadTooLarge, [ErrorCodes.PayloadTooLarge]);
            Add(StatusCodes.Status415UnsupportedMediaType, [ErrorCodes.ValidationError]);
        }

        // An expired or revoked token, or one of a suspended or deleted account (LLD §7.1)
        if (!action.EndpointMetadata.OfType<IAllowAnonymous>().Any())
        {
            Add(StatusCodes.Status401Unauthorized, [ErrorCodes.InvalidToken]);
        }

        foreach (var declared in action.EndpointMetadata.OfType<ApiErrorAttribute>())
        {
            Add(declared.StatusCode, declared.Codes);
        }

        return errors
            .OrderBy(e => Array.IndexOf(_statusOrder, e.Key) is var index and >= 0 ? index : int.MaxValue)
            .ThenBy(e => e.Key)
            .Select(e => (e.Key, e.Value));
    }

    // RFC 7807 plus the extensions every error carries (ProblemDetailsSetup) and the per-field errors of a 400
    private static OpenApiSchema BuildProblemSchema() => new()
    {
        Type = JsonSchemaType.Object,
        Description = "Error in RFC 7807 format. The app translates `code` into a Spanish message (LLD §3.4).",
        Required = new HashSet<string> { "status", "code", "traceId" },
        Properties = new Dictionary<string, IOpenApiSchema>
        {
            ["type"] = new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null },
            ["title"] = new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null },
            ["status"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
            ["detail"] = new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null },
            ["instance"] = new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null },
            ["code"] = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Enum = [.. AllErrorCodes().Select(code => (JsonNode)JsonValue.Create(code)!)],
            },
            ["traceId"] = new OpenApiSchema { Type = JsonSchemaType.String },
            ["errors"] = new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Description = "Only in 400 VALIDATION_ERROR: messages per field, in camelCase.",
                AdditionalProperties = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = new OpenApiSchema { Type = JsonSchemaType.String },
                },
            },
        },
    };

    public static IEnumerable<string> AllErrorCodes() =>
        typeof(ErrorCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue()!);
}
