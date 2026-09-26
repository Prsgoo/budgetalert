using BudgetAlert.Application.Contracts;
using BudgetAlert.Domain.Repositories;
using BudgetAlert.Infrastructure.Messaging;
using BudgetAlert.Infrastructure.Persistence;
using BudgetAlert.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetAlert.Infrastructure.Extensions
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<BudgetAlertDbContext>(opts =>
                opts.UseSqlServer(config.GetConnectionString("Default")));

            services.Configure<RabbitMqOptions>(config.GetSection("RabbitMq"));
            services.AddSingleton<IEventBus, RabbitMqEventBus>();

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<BudgetAlertDbContext>());
            services.AddScoped<IBudgetRepository, BudgetRepository>();
            services.AddScoped<IAlertRepository, AlertRepository>();

            return services;
        }
    }

}