using System.Text.Json;
using BudgetAlert.Domain.Events;
using BudgetAlert.Infrastructure.Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BudgetAlert.Worker
{
    public class TransactionEventConsumer : BackgroundService
    {

        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<TransactionEventConsumer> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public TransactionEventConsumer(IOptions<RabbitMqOptions> options, IServiceScopeFactory scopeFactory, ILogger<TransactionEventConsumer> logger)
        {
            _options = options.Value;
            _logger = logger;
            _scopeFactory = scopeFactory;

            try
            {
                (_connection, _channel) = RabbitMqChannelFactory.Create(_options);
                _channel.QueueDeclareAsync("budget-alert.transaction-registered", durable: true, exclusive: false, autoDelete: false)
                    .GetAwaiter().GetResult();
                _channel.QueueBindAsync("budget-alert.transaction-registered", _options.ExchangeName, "transactionregistered")
                    .GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to RabbitMQ at {Host}:{Port}", _options.Host, _options.Port);
                throw;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var evt = JsonSerializer.Deserialize<TransactionRegistered>(ea.Body.ToArray());
                    using var scope = _scopeFactory.CreateScope();
                    var evaluator = scope.ServiceProvider.GetRequiredService<AlertRuleEvaluator>();
                    await evaluator.EvaluateAsync(evt!, stoppingToken);
                    await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process message");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
                }
            };

            await _channel.BasicConsumeAsync("budget-alert.transaction-registered", false, consumer, stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}