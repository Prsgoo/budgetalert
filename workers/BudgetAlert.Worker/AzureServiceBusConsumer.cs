using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using BudgetAlert.Domain.Events;
using BudgetAlert.Infrastructure.Messaging;
using Microsoft.Extensions.Options;

namespace BudgetAlert.Worker
{
    [ExcludeFromCodeCoverage]
    public class AzureServiceBusConsumer(
        ServiceBusClient client,
        IOptions<AzureServiceBusOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<AzureServiceBusConsumer> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var processor = client.CreateProcessor(options.Value.QueueName);

            processor.ProcessMessageAsync += async args =>
            {
                try
                {
                    if (args.Message.Subject == nameof(TransactionRegistered))
                    {
                        var evt = JsonSerializer.Deserialize<TransactionRegistered>(args.Message.Body.ToArray());
                        using var scope = scopeFactory.CreateScope();
                        var evaluator = scope.ServiceProvider.GetRequiredService<AlertRuleEvaluator>();
                        await evaluator.EvaluateAsync(evt!, stoppingToken);
                    }
                    await args.CompleteMessageAsync(args.Message, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to process message {MessageId}", args.Message.MessageId);
                    await args.AbandonMessageAsync(args.Message, cancellationToken: stoppingToken);
                }
            };

            processor.ProcessErrorAsync += args =>
            {
                logger.LogError(args.Exception, "Azure Service Bus processor error on {EntityPath}", args.EntityPath);
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync(stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
            await processor.StopProcessingAsync();
            await processor.DisposeAsync();
        }
    }
}
