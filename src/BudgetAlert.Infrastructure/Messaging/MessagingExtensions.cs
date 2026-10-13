using BudgetAlert.Application.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            // Intentional stub - returns services unchanged until PRS-101 fills this in.
            // The app will start but fail at runtime when messaging is attempted.
            return services;
        }
    }
}
