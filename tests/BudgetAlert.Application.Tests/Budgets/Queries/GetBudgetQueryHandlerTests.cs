using BudgetAlert.Application.Budgets.Queries;
using BudgetAlert.Application.Exceptions;
using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Repositories;
using Moq;

namespace BudgetAlert.Application.Tests.Budgets.Queries;

public class GetBudgetQueryHandlerTests
{
    private readonly Mock<IBudgetRepository> _repo = new();

    [Fact]
    public async Task Handle_ThrowsNotFoundExceptionWhenBudgetNotFound()
    {
        _repo.Setup(r => r.GetByIdWithTransactionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Budget?)null);
        var handler = new GetBudgetQueryHandler(_repo.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetBudgetQuery(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_MapsIdToDto()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        _repo.Setup(r => r.GetByIdWithTransactionsAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new GetBudgetQueryHandler(_repo.Object);

        var dto = await handler.Handle(new GetBudgetQuery(budget.Id), CancellationToken.None);

        Assert.Equal(budget.Id, dto.Id);
    }

    [Fact]
    public async Task Handle_MapsNameLimitCurrencyToDto()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        _repo.Setup(r => r.GetByIdWithTransactionsAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new GetBudgetQueryHandler(_repo.Object);

        var dto = await handler.Handle(new GetBudgetQuery(budget.Id), CancellationToken.None);

        Assert.Equal("Monthly", dto.Name);
        Assert.Equal(1000m, dto.Limit);
        Assert.Equal("EUR", dto.Currency);
    }

    [Fact]
    public async Task Handle_CalculatesCurrentSpendCorrectly()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        budget.RegisterTransaction(200m, "A", DateTime.UtcNow);
        budget.RegisterTransaction(300m, "B", DateTime.UtcNow);
        _repo.Setup(r => r.GetByIdWithTransactionsAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new GetBudgetQueryHandler(_repo.Object);

        var dto = await handler.Handle(new GetBudgetQuery(budget.Id), CancellationToken.None);

        Assert.Equal(500m, dto.CurrentSpend);
    }

    [Fact]
    public async Task Handle_CalculatesSpendPercentageCorrectly()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        budget.RegisterTransaction(800m, "A", DateTime.UtcNow);
        _repo.Setup(r => r.GetByIdWithTransactionsAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new GetBudgetQueryHandler(_repo.Object);

        var dto = await handler.Handle(new GetBudgetQuery(budget.Id), CancellationToken.None);

        Assert.Equal(80m, dto.SpendPercentage);
    }

    [Fact]
    public async Task Handle_ReturnsAtMost10Transactions()
    {
        var budget = Budget.Create("Monthly", 5000m, "EUR");
        for (var i = 0; i < 12; i++)
            budget.RegisterTransaction(10m, $"T{i}", DateTime.UtcNow.AddDays(-i));
        _repo.Setup(r => r.GetByIdWithTransactionsAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new GetBudgetQueryHandler(_repo.Object);

        var dto = await handler.Handle(new GetBudgetQuery(budget.Id), CancellationToken.None);

        Assert.Equal(10, dto.RecentTransactions.Count);
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundExceptionWhenBudgetIsArchived()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        budget.Archive();
        _repo.Setup(r => r.GetByIdWithTransactionsAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new GetBudgetQueryHandler(_repo.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetBudgetQuery(budget.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ReturnsTransactionsOrderedByOccurredAtDescending()
    {
        var budget = Budget.Create("Monthly", 5000m, "EUR");
        var baseDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        budget.RegisterTransaction(10m, "Oldest", baseDate.AddDays(-2));
        budget.RegisterTransaction(10m, "Middle", baseDate.AddDays(-1));
        budget.RegisterTransaction(10m, "Newest", baseDate);
        _repo.Setup(r => r.GetByIdWithTransactionsAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new GetBudgetQueryHandler(_repo.Object);

        var dto = await handler.Handle(new GetBudgetQuery(budget.Id), CancellationToken.None);

        Assert.Equal("Newest", dto.RecentTransactions[0].Description);
        Assert.Equal("Oldest", dto.RecentTransactions[2].Description);
    }
}
