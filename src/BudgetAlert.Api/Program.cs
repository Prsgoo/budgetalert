using BudgetAlert.Application.Extensions;
using BudgetAlert.Infrastructure.Extensions;
using BudgetAlert.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using BudgetAlert.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

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
});

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
app.UseSwaggerUI();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();
app.Run();

public partial class Program { }