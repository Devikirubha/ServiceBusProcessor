using API.Extensions;
using API.Middleware;
using Infrastructure;
using Application;
using Serilog;
using CorrelationId;
using CorrelationId.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId());

// ── Layers ─────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);


builder.Services.AddDefaultCorrelationId(options =>
{
    options.AddToLoggingScope = true;
    options.EnforceHeader = false;
    options.IgnoreRequestHeader = false;
    options.IncludeInResponse = true;
    options.UpdateTraceIdentifier = true;
});


builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddScopeAuthorization();
builder.Services.AddApiVersioningConfiguration();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddHealthChecksConfiguration(builder.Configuration);
builder.Services.AddControllers();
var app = builder.Build();

#region Middleware

app.UseCorrelationId();
app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();

#endregion

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("TST"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
        options.OAuthClientId("swagger-client");
        options.OAuthUsePkce();
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();

public partial class Program { } // for integration tests
