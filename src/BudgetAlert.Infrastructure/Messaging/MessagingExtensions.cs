using Azure.Identity;
using Azure.Messaging.ServiceBus;
using BudgetAlert.Application.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BudgetAlert.Infrastructure.Messaging
{
    public static class MessagingExtensions
    {
        public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
        {
            var provider = configuration["Messaging:Provider"]
                ?? throw new InvalidOperationException("Messaging:Provider is not configured.");

            return provider switch
            {
                "RabbitMQ" => services.AddRabbitMqMessaging(configuration),
                "AzureServiceBus" => services.AddAzureServiceBusMessaging(configuration),
                _ => throw new InvalidOperationException($"Unknown messaging provider: {provider}")
            };
        }

        private static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
            services.AddSingleton<IEventBus, RabbitMqEventBus>();
            return services;
        }

        private static IServiceCollection AddAzureServiceBusMessaging(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AzureServiceBusOptions>(configuration.GetSection("Messaging:AzureServiceBus"));
            services.AddSingleton(sp =>
            {
                var opts = sp.GetRequiredService<IOptions<AzureServiceBusOptions>>().Value;
                return !string.IsNullOrEmpty(opts.FullyQualifiedNamespace)
                    ? new ServiceBusClient(opts.FullyQualifiedNamespace, new DefaultAzureCredential())
                    : new ServiceBusClient(opts.ConnectionString);
            });
            services.AddSingleton<IEventBus, AzureServiceBusEventBus>();
            return services;
        }
    }
}
