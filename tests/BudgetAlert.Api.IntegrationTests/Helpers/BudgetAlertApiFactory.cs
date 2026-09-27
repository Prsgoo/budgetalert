using BudgetAlert.Application.Contracts;
using BudgetAlert.Domain.Events;
using BudgetAlert.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;

namespace BudgetAlert.Api.IntegrationTests.Helpers;

public class BudgetAlertApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _db = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public FakeEventBus EventBus { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BudgetAlertDbContext>));
            if (descriptor != null) services.Remove(descriptor);
            services.AddDbContext<BudgetAlertDbContext>(opts =>
                opts.UseSqlServer(_db.GetConnectionString()));

            services.RemoveAll<IEventBus>();
            services.AddSingleton<IEventBus>(EventBus);
        });
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _db.StartAsync();
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BudgetAlertDbContext>();
        await db.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _db.DisposeAsync();
        Dispose();
    }
}

public class FakeEventBus : IEventBus
{
    public List<IDomainEvent> Published { get; } = [];

    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IDomainEvent
    {
        Published.Add(@event);
        return Task.CompletedTask;
    }
}
