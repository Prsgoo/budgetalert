using BudgetAlert.Infrastructure.Extensions;
using BudgetAlert.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<AlertRuleEvaluator>();
builder.Services.AddHostedService<TransactionEventConsumer>();

var host = builder.Build();
host.Run();
