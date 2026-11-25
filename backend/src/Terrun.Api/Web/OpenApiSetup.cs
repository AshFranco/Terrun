using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;

namespace Terrun.Api.Web;

// The OpenAPI document is the source of the app types (CTZ-19). It declares the JWT scheme and marks every controller
// action that needs a token, matching MapControllers().RequireAuthorization() in Program.cs
internal static class OpenApiSetup
{
    public const string BearerScheme = "Bearer";

    public static IServiceCollection AddApiOpenApi(this IServiceCollection services) =>
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes[BearerScheme] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Access token from /api/v1/auth/login or /api/v1/auth/refresh (15 minutes).",
                };
                return Task.CompletedTask;
            });
            options.AddDocumentTransformer(OpenApiErrorResponses.AddProblemSchemaAsync);
            options.AddOperationTransformer(OpenApiErrorResponses.AddErrorResponsesAsync);
            options.AddOperationTransformer((operation, context, _) =>
            {
                if (context.Description.ActionDescriptor is ControllerActionDescriptor action
                    && !action.EndpointMetadata.OfType<IAllowAnonymous>().Any())
                {
                    operation.Security ??= [];
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference(BearerScheme, context.Document)] = [],
                    });
                }

                return Task.CompletedTask;
            });
        });
}
