using BudgetAlert.Application.Budgets.Queries;
using BudgetAlert.Application.Exceptions;
using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Repositories;
using Moq;

namespace BudgetAlert.Application.Tests.Budgets.Queries;

public class GetAlertRulesQueryHandlerTests
{
    private readonly Mock<IBudgetRepository> _repo = new();

    [Fact]
    public async Task Handle_ThrowsNotFoundExceptionWhenBudgetNotFound()
    {
        _repo.Setup(r => r.GetByIdWithAlertRulesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Budget?)null);
        var handler = new GetAlertRulesQueryHandler(_repo.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetAlertRulesQuery(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ReturnsAllRulesForBudget()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        budget.AddAlertRule(50m);
        budget.AddAlertRule(80m);
        _repo.Setup(r => r.GetByIdWithAlertRulesAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new GetAlertRulesQueryHandler(_repo.Object);

        var result = await handler.Handle(new GetAlertRulesQuery(budget.Id), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_MapsThresholdAndIsActive()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        budget.AddAlertRule(75m);
        _repo.Setup(r => r.GetByIdWithAlertRulesAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new GetAlertRulesQueryHandler(_repo.Object);

        var result = await handler.Handle(new GetAlertRulesQuery(budget.Id), CancellationToken.None);

        Assert.Equal(75m, result[0].ThresholdPercentage);
        Assert.True(result[0].IsActive);
    }
}
