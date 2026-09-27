using BudgetAlert.Application.Contracts;
using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Events;
using BudgetAlert.Domain.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetAlert.Api.Tests.Helpers;

public class ApiTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IEventBus, NoOpEventBus>();
            services.AddSingleton<IBudgetRepository, FakeBudgetRepository>();
            services.AddSingleton<IAlertRepository, FakeAlertRepository>();
            services.AddSingleton<IUnitOfWork, FakeUnitOfWork>();
        });
    }
}

public class FakeBudgetRepository : IBudgetRepository
{
    private readonly Dictionary<Guid, Budget> _store = new();

    public void Add(Budget budget) => _store[budget.Id] = budget;
    public Task<Budget?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.TryGetValue(id, out var b) ? b : null);
    public Task<Budget?> GetByIdWithTransactionsAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.TryGetValue(id, out var b) ? b : null);
    public Task<Budget?> GetByIdWithAlertRulesAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.TryGetValue(id, out var b) ? b : null);
}

public class FakeAlertRepository : IAlertRepository
{
    public void Add(Alert alert) { }
    public Task<IReadOnlyList<Alert>> GetAsync(Guid? budgetId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Alert>>([]);
}

public class FakeUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    public void Register<T>(T entity) where T : class { }
}

public class NoOpEventBus : IEventBus
{
    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IDomainEvent
        => Task.CompletedTask;
}
