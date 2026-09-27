using BudgetAlert.Application.Budgets.Commands;
using BudgetAlert.Application.Exceptions;
using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Repositories;
using Moq;

namespace BudgetAlert.Application.Tests.Budgets.Commands;

public class AddAlertRuleCommandHandlerTests
{
    private readonly Mock<IBudgetRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Handle_ThrowsNotFoundExceptionWhenBudgetNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Budget?)null);
        var handler = new AddAlertRuleCommandHandler(_repo.Object, _unitOfWork.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new AddAlertRuleCommand(Guid.NewGuid(), 80m), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ReturnsNonEmptyAlertRuleId()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        _repo.Setup(r => r.GetByIdAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new AddAlertRuleCommandHandler(_repo.Object, _unitOfWork.Object);

        var ruleId = await handler.Handle(new AddAlertRuleCommand(budget.Id, 80m), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, ruleId);
    }

    [Fact]
    public void Validator_RejectsEmptyBudgetId()
    {
        var validator = new AddAlertRuleCommandValidator();
        var result = validator.Validate(new AddAlertRuleCommand(Guid.Empty, 80m));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AddAlertRuleCommand.BudgetId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validator_RejectsThresholdOutOfRange(decimal threshold)
    {
        var validator = new AddAlertRuleCommandValidator();
        var result = validator.Validate(new AddAlertRuleCommand(Guid.NewGuid(), threshold));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AddAlertRuleCommand.ThresholdPercentage));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void Validator_AcceptsBoundaryThresholds(decimal threshold)
    {
        var validator = new AddAlertRuleCommandValidator();
        var result = validator.Validate(new AddAlertRuleCommand(Guid.NewGuid(), threshold));
        Assert.True(result.IsValid);
    }
}
