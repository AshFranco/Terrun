using Terrun.Api.Modules.Identity;
using Terrun.Api.Modules.Platform;
using Terrun.Api.Modules.Profiles;
using Terrun.Api.Web;
using Terrun.Infrastructure;
using Terrun.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Db")
    ?? throw new InvalidOperationException("Missing connection string 'ConnectionStrings:Db'.");

builder.AddApiConventions();
builder.Services.AddApiOpenApi();
builder.Services.AddInfrastructure(connectionString, builder.Configuration);
builder.Services.AddPlatformModule();
builder.AddIdentityModule();
builder.Services.AddProfilesModule();

var app = builder.Build();

// QA and production apply pending migrations on every deploy; tests turn this off to control the database themselves
if (app.Configuration.GetValue<bool>(DatabaseMigrator.MigrateOnStartupKey))
{
    await app.Services.MigrateDatabaseAsync();
}

app.UseApiConventions();

// LLD §7.3: Swagger is exposed only outside production
if (!app.Environment.IsProduction())
{
    app.MapOpenApi().AllowAnonymous();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Terrun API v1"));
}

// Secure by default: every controller action needs a valid token unless it opts out with [AllowAnonymous].
// Applied to the controllers, not as a fallback policy, so unknown routes still answer 404 and Swagger UI loads
app.MapControllers().RequireAuthorization();
app.MapPlatformModule();

app.Run();
