using Terrun.Api.Modules.Platform;
using Terrun.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Db")
    ?? throw new InvalidOperationException("Missing connection string 'ConnectionStrings:Db'.");

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddPlatformModule();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// LLD §7.3: Swagger is exposed only outside production
if (!app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Terrun API v1"));
}

app.MapControllers();
app.MapPlatformModule();

app.Run();
