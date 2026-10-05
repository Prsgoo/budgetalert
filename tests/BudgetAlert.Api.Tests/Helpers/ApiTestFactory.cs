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
    public FakeAlertRepository Alerts { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IEventBus, NoOpEventBus>();
            services.AddSingleton<IBudgetRepository, FakeBudgetRepository>();
            services.AddSingleton<IAlertRepository>(Alerts);
            services.AddSingleton<IUnitOfWork, FakeUnitOfWork>();
        });
    }
}

public class FakeBudgetRepository : IBudgetRepository
{
    private readonly Dictionary<Guid, Budget> _store = [];

    public void Add(Budget budget) => _store[budget.Id] = budget;
    public void AddAlertRule(AlertRule alertRule) { } // already on budget's in-memory collection
    public void AddTransaction(Transaction transaction) { } // already on budget's in-memory collection
    public Task<Budget?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.TryGetValue(id, out var b) ? b : null);
    public Task<Budget?> GetByIdWithTransactionsAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.TryGetValue(id, out var b) ? b : null);
    public Task<Budget?> GetByIdWithAlertRulesAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.TryGetValue(id, out var b) ? b : null);
}

public class FakeAlertRepository : IAlertRepository
{
    private readonly List<Alert> _store = [];

    public void Add(Alert alert) => _store.Add(alert);
    public Task<IReadOnlyList<Alert>> GetAsync(Guid? budgetId, CancellationToken cancellationToken = default)
    {
        var result = budgetId.HasValue
            ? _store.Where(a => a.BudgetId == budgetId.Value).ToList()
            : _store.ToList();
        return Task.FromResult<IReadOnlyList<Alert>>(result);
    }
}

public class FakeUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
}

public class NoOpEventBus : IEventBus
{
    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IDomainEvent
        => Task.CompletedTask;
}
