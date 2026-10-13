using BudgetAlert.Application.Extensions;
using BudgetAlert.Infrastructure.Extensions;
using BudgetAlert.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;
using BudgetAlert.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

var authEnabled = builder.Configuration.GetValue<bool>("Auth:Enabled");
var tenantId = builder.Configuration["Auth:AzureAd:TenantId"] ?? string.Empty;
var clientId = builder.Configuration["Auth:AzureAd:ClientId"] ?? string.Empty;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "BudgetAlert API",
        Version = "v1",
        Description = "Event-driven budget alerting - Clean Architecture + CQRS demo"
    });

    if (authEnabled)
    {
        c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize"),
                    TokenUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        { $"api://{clientId}/access_as_user", "Access BudgetAlert API" }
                    }
                }
            }
        });

        c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("oauth2", doc, null),
                new List<string> { $"api://{clientId}/access_as_user" }
            }
        });
    }
});

if (authEnabled)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("Auth:AzureAd"));

    builder.Services.AddAuthorization();
}
else
{
    // Always register authorization so [Authorize] on controllers doesn't crash at runtime.
    // The permissive default policy makes [Authorize] a no-op when auth is disabled.
    builder.Services.AddAuthorization(options =>
        options.DefaultPolicy = new AuthorizationPolicyBuilder()
            .RequireAssertion(_ => true)
            .Build());
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BudgetAlertDbContext>();
    await db.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    if (authEnabled)
    {
        c.OAuthClientId(clientId);
        c.OAuthUsePkce();
    }
});
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (authEnabled)
{
    app.UseAuthentication();
    app.UseAuthorization();
}
else
{
    app.UseAuthorization();
}

app.MapControllers();
app.Run();

public partial class Program { }
