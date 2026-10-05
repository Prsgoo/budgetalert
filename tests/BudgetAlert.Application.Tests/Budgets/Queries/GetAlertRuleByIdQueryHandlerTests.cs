using BudgetAlert.Application.Budgets.Queries;
using BudgetAlert.Application.Exceptions;
using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Repositories;
using Moq;

namespace BudgetAlert.Application.Tests.Budgets.Queries;

public class GetAlertRuleByIdQueryHandlerTests
{
    private readonly Mock<IBudgetRepository> _repo = new();

    [Fact]
    public async Task Handle_ThrowsNotFoundExceptionWhenBudgetNotFound()
    {
        _repo.Setup(r => r.GetByIdWithAlertRulesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Budget?)null);
        var handler = new GetAlertRuleByIdQueryHandler(_repo.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetAlertRuleByIdQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundExceptionWhenRuleNotFound()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        _repo.Setup(r => r.GetByIdWithAlertRulesAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new GetAlertRuleByIdQueryHandler(_repo.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetAlertRuleByIdQuery(budget.Id, Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ReturnsMappedDto()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        var rule = budget.AddAlertRule(80m);
        _repo.Setup(r => r.GetByIdWithAlertRulesAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new GetAlertRuleByIdQueryHandler(_repo.Object);

        var dto = await handler.Handle(new GetAlertRuleByIdQuery(budget.Id, rule.Id), CancellationToken.None);

        Assert.Equal(rule.Id, dto.Id);
        Assert.Equal(budget.Id, dto.BudgetId);
        Assert.Equal(80m, dto.ThresholdPercentage);
        Assert.True(dto.IsActive);
    }
}
