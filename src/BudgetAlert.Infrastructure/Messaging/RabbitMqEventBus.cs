using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using BudgetAlert.Application.Contracts;
using BudgetAlert.Domain.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace BudgetAlert.Infrastructure.Messaging
{
    [ExcludeFromCodeCoverage]
    public class RabbitMqEventBus : IEventBus, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly RabbitMqOptions _options;

        public RabbitMqEventBus(IOptions<RabbitMqOptions> options, ILogger<RabbitMqEventBus> logger)
        {
            _options = options.Value;
            try
            {
                (_connection, _channel) = RabbitMqChannelFactory.Create(_options);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to RabbitMQ at {Host}:{Port}", _options.Host, _options.Port);
                throw;
            }
        }

        public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IDomainEvent
        {
            var eventType = @event.GetType();
            var body = JsonSerializer.SerializeToUtf8Bytes(@event, eventType);
            var routingKey = eventType.Name.ToLowerInvariant();
            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent
            };

            await _channel.BasicPublishAsync(_options.ExchangeName, routingKey, false, props, body, cancellationToken);
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}