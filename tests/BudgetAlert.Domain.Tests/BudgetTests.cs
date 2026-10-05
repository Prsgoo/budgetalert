using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Events;

namespace BudgetAlert.Domain.Tests;

public class BudgetTests
{
    [Fact]
    public void Create_SetsName()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        Assert.Equal("Monthly", budget.Name);
    }

    [Fact]
    public void Create_SetsLimit()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        Assert.Equal(1000m, budget.Limit);
    }

    [Fact]
    public void Create_SetsCurrency()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        Assert.Equal("EUR", budget.Currency);
    }

    [Fact]
    public void Create_GeneratesNonEmptyId()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        Assert.NotEqual(Guid.Empty, budget.Id);
    }

    [Fact]
    public void Create_SetsCreatedAtToApproximatelyNow()
    {
        var before = DateTime.UtcNow;
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        var after = DateTime.UtcNow;

        Assert.InRange(budget.CreatedAt, before, after);
    }

    [Fact]
    public void RegisterTransaction_AddsTransactionToCollection()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");

        budget.RegisterTransaction(100m, "Groceries", DateTime.UtcNow);

        Assert.Single(budget.Transactions);
    }

    [Fact]
    public void RegisterTransaction_RaisesTransactionRegisteredEvent()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");

        budget.RegisterTransaction(100m, "Groceries", DateTime.UtcNow);

        Assert.Single(budget.DomainEvents);
        Assert.IsType<TransactionRegistered>(budget.DomainEvents[0]);
    }

    [Fact]
    public void RegisterTransaction_EventContainsBudgetId()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");

        budget.RegisterTransaction(100m, "Groceries", DateTime.UtcNow);

        var evt = (TransactionRegistered)budget.DomainEvents[0];
        Assert.Equal(budget.Id, evt.BudgetId);
    }

    [Fact]
    public void RegisterTransaction_EventContainsCorrectAmount()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");

        budget.RegisterTransaction(250m, "Groceries", DateTime.UtcNow);

        var evt = (TransactionRegistered)budget.DomainEvents[0];
        Assert.Equal(250m, evt.Amount);
    }

    [Fact]
    public void RegisterTransaction_EventContainsCurrentSpendAfterTransaction()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        budget.RegisterTransaction(100m, "First", DateTime.UtcNow);
        budget.PopDomainEvents();

        budget.RegisterTransaction(200m, "Second", DateTime.UtcNow);

        var evt = (TransactionRegistered)budget.DomainEvents[0];
        Assert.Equal(300m, evt.CurrentSpend);
    }

    [Fact]
    public void RegisterTransaction_EventContainsBudgetLimit()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");

        budget.RegisterTransaction(100m, "Groceries", DateTime.UtcNow);

        var evt = (TransactionRegistered)budget.DomainEvents[0];
        Assert.Equal(1000m, evt.BudgetLimit);
    }

    [Fact]
    public void CurrentSpend_SumsAllTransactionAmounts()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        budget.RegisterTransaction(100m, "A", DateTime.UtcNow);
        budget.RegisterTransaction(250m, "B", DateTime.UtcNow);

        Assert.Equal(350m, budget.CurrentSpend);
    }

    [Fact]
    public void CurrentSpend_ReturnsZeroWhenNoTransactions()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");

        Assert.Equal(0m, budget.CurrentSpend);
    }

    [Fact]
    public void SpendPercentage_CalculatesCorrectly()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        budget.RegisterTransaction(800m, "A", DateTime.UtcNow);

        Assert.Equal(80m, budget.SpendPercentage);
    }

    [Fact]
    public void SpendPercentage_ReturnsZeroWhenLimitIsZero()
    {
        var budget = Budget.Create("Monthly", 0m, "EUR");

        Assert.Equal(0m, budget.SpendPercentage);
    }

    [Fact]
    public void AddAlertRule_AddsRuleToCollection()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");

        budget.AddAlertRule(80m);

        Assert.Single(budget.AlertRules);
    }

    [Fact]
    public void AddAlertRule_ReturnsNonEmptyId()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");

        var rule = budget.AddAlertRule(80m);

        Assert.NotEqual(Guid.Empty, rule.Id);
    }

    [Fact]
    public void AddAlertRule_ReturnedIdMatchesRuleInCollection()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");

        var rule = budget.AddAlertRule(80m);

        Assert.Equal(rule.Id, budget.AlertRules.Single().Id);
    }

    [Fact]
    public void PopDomainEvents_ReturnsAccumulatedEvents()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        budget.RegisterTransaction(100m, "A", DateTime.UtcNow);

        var events = budget.PopDomainEvents();

        Assert.Single(events);
    }

    [Fact]
    public void PopDomainEvents_ClearsEventsList()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        budget.RegisterTransaction(100m, "A", DateTime.UtcNow);
        budget.PopDomainEvents();

        Assert.Empty(budget.DomainEvents);
    }

    [Fact]
    public void Create_IsNotArchivedByDefault()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        Assert.False(budget.IsArchived);
    }

    [Fact]
    public void Archive_SetsIsArchivedToTrue()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        budget.Archive();
        Assert.True(budget.IsArchived);
    }

    [Fact]
    public void UpdateDetails_UpdatesNameLimitAndCurrency()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        budget.UpdateDetails("Annual", 5000m, "USD");
        Assert.Equal("Annual", budget.Name);
        Assert.Equal(5000m, budget.Limit);
        Assert.Equal("USD", budget.Currency);
    }
}
