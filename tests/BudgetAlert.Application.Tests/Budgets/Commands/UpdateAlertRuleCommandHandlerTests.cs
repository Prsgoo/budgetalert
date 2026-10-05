using BudgetAlert.Application.Budgets.Commands;
using BudgetAlert.Application.Exceptions;
using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Repositories;
using Moq;

namespace BudgetAlert.Application.Tests.Budgets.Commands;

public class UpdateAlertRuleCommandHandlerTests
{
    private readonly Mock<IBudgetRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Handle_ThrowsNotFoundExceptionWhenBudgetNotFound()
    {
        _repo.Setup(r => r.GetByIdWithAlertRulesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Budget?)null);
        var handler = new UpdateAlertRuleCommandHandler(_repo.Object, _unitOfWork.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new UpdateAlertRuleCommand(Guid.NewGuid(), Guid.NewGuid(), 50m, true), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundExceptionWhenRuleNotFound()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        _repo.Setup(r => r.GetByIdWithAlertRulesAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new UpdateAlertRuleCommandHandler(_repo.Object, _unitOfWork.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new UpdateAlertRuleCommand(budget.Id, Guid.NewGuid(), 50m, true), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UpdatesAlertRule()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        var rule = budget.AddAlertRule(80m);
        _repo.Setup(r => r.GetByIdWithAlertRulesAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new UpdateAlertRuleCommandHandler(_repo.Object, _unitOfWork.Object);

        await handler.Handle(new UpdateAlertRuleCommand(budget.Id, rule.Id, 50m, false), CancellationToken.None);

        Assert.Equal(50m, rule.ThresholdPercentage);
        Assert.False(rule.IsActive);
    }

    [Fact]
    public async Task Handle_SavesChanges()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        var rule = budget.AddAlertRule(80m);
        _repo.Setup(r => r.GetByIdWithAlertRulesAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new UpdateAlertRuleCommandHandler(_repo.Object, _unitOfWork.Object);

        await handler.Handle(new UpdateAlertRuleCommand(budget.Id, rule.Id, 50m, false), CancellationToken.None);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Validator_RejectsEmptyBudgetId()
    {
        var validator = new UpdateAlertRuleCommandValidator();
        var result = validator.Validate(new UpdateAlertRuleCommand(Guid.Empty, Guid.NewGuid(), 50m, true));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateAlertRuleCommand.BudgetId));
    }

    [Fact]
    public void Validator_RejectsEmptyAlertRuleId()
    {
        var validator = new UpdateAlertRuleCommandValidator();
        var result = validator.Validate(new UpdateAlertRuleCommand(Guid.NewGuid(), Guid.Empty, 50m, true));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateAlertRuleCommand.AlertRuleId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validator_RejectsThresholdOutOfRange(decimal threshold)
    {
        var validator = new UpdateAlertRuleCommandValidator();
        var result = validator.Validate(new UpdateAlertRuleCommand(Guid.NewGuid(), Guid.NewGuid(), threshold, true));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateAlertRuleCommand.ThresholdPercentage));
    }
}
