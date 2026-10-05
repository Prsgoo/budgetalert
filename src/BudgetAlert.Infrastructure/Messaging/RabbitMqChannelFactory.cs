using System.Diagnostics.CodeAnalysis;
using RabbitMQ.Client;

namespace BudgetAlert.Infrastructure.Messaging
{
    [ExcludeFromCodeCoverage]
    public static class RabbitMqChannelFactory
    {
        // Sync-over-async is intentional here: constructors can't be async, and RabbitMQ
        // connections are established once at startup before the host begins serving traffic.
        public static (IConnection connection, IChannel channel) Create(RabbitMqOptions options)
        {
            var factory = new ConnectionFactory
            {
                HostName = options.Host,
                Port = options.Port,
                UserName = options.Username,
                Password = options.Password
            };

            var connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            var channel = connection.CreateChannelAsync().GetAwaiter().GetResult();
            channel.ExchangeDeclareAsync(options.ExchangeName, ExchangeType.Topic, durable: true)
                .GetAwaiter().GetResult();

            return (connection, channel);
        }
    }
}
