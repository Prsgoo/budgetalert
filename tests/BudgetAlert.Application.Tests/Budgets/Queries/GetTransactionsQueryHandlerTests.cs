using BudgetAlert.Application.Budgets.Queries;
using BudgetAlert.Application.Exceptions;
using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Repositories;
using Moq;

namespace BudgetAlert.Application.Tests.Budgets.Queries;

public class GetTransactionsQueryHandlerTests
{
    private readonly Mock<IBudgetRepository> _repo = new();

    [Fact]
    public async Task Handle_ThrowsNotFoundExceptionWhenBudgetNotFound()
    {
        _repo.Setup(r => r.GetByIdWithTransactionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Budget?)null);
        var handler = new GetTransactionsQueryHandler(_repo.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetTransactionsQuery(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ReturnsAllTransactions()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        budget.RegisterTransaction(100m, "A", DateTime.UtcNow);
        budget.RegisterTransaction(200m, "B", DateTime.UtcNow);
        _repo.Setup(r => r.GetByIdWithTransactionsAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new GetTransactionsQueryHandler(_repo.Object);

        var result = await handler.Handle(new GetTransactionsQuery(budget.Id), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_ReturnsTransactionsOrderedByOccurredAtDescending()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        var baseDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        budget.RegisterTransaction(10m, "Oldest", baseDate.AddDays(-2));
        budget.RegisterTransaction(10m, "Newest", baseDate);
        _repo.Setup(r => r.GetByIdWithTransactionsAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new GetTransactionsQueryHandler(_repo.Object);

        var result = await handler.Handle(new GetTransactionsQuery(budget.Id), CancellationToken.None);

        Assert.Equal("Newest", result[0].Description);
        Assert.Equal("Oldest", result[1].Description);
    }
}
