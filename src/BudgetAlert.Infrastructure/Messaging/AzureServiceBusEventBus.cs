using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using BudgetAlert.Application.Contracts;
using BudgetAlert.Domain.Events;
using Microsoft.Extensions.Options;

namespace BudgetAlert.Infrastructure.Messaging
{
    [ExcludeFromCodeCoverage]
    public class AzureServiceBusEventBus(ServiceBusClient client, IOptions<AzureServiceBusOptions> options) : IEventBus
    {
        public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IDomainEvent
        {
            var sender = client.CreateSender(options.Value.QueueName);
            var body = JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType());
            var message = new ServiceBusMessage(body)
            {
                Subject = typeof(T).Name
            };
            await sender.SendMessageAsync(message, cancellationToken);
        }
    }
}
