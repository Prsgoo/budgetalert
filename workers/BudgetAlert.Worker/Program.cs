using BudgetAlert.Infrastructure.Extensions;
using BudgetAlert.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<AlertRuleEvaluator>();

var messagingProvider = builder.Configuration["Messaging:Provider"] ?? "RabbitMQ";
if (messagingProvider == "AzureServiceBus")
    builder.Services.AddHostedService<AzureServiceBusConsumer>();
else
    builder.Services.AddHostedService<TransactionEventConsumer>();

var host = builder.Build();
host.Run();
