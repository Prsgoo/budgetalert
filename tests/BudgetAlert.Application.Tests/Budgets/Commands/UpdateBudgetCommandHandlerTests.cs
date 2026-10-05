using BudgetAlert.Application.Budgets.Commands;
using BudgetAlert.Application.Exceptions;
using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Repositories;
using Moq;

namespace BudgetAlert.Application.Tests.Budgets.Commands;

public class UpdateBudgetCommandHandlerTests
{
    private readonly Mock<IBudgetRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Handle_ThrowsNotFoundExceptionWhenBudgetNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Budget?)null);
        var handler = new UpdateBudgetCommandHandler(_repo.Object, _unitOfWork.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new UpdateBudgetCommand(Guid.NewGuid(), "New", 500m, "USD"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundExceptionWhenBudgetIsArchived()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        budget.Archive();
        _repo.Setup(r => r.GetByIdAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new UpdateBudgetCommandHandler(_repo.Object, _unitOfWork.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new UpdateBudgetCommand(budget.Id, "New", 500m, "USD"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UpdatesBudgetDetails()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        _repo.Setup(r => r.GetByIdAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new UpdateBudgetCommandHandler(_repo.Object, _unitOfWork.Object);

        await handler.Handle(new UpdateBudgetCommand(budget.Id, "Quarterly", 2000m, "USD"), CancellationToken.None);

        Assert.Equal("Quarterly", budget.Name);
        Assert.Equal(2000m, budget.Limit);
        Assert.Equal("USD", budget.Currency);
    }

    [Fact]
    public async Task Handle_SavesChanges()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        _repo.Setup(r => r.GetByIdAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new UpdateBudgetCommandHandler(_repo.Object, _unitOfWork.Object);

        await handler.Handle(new UpdateBudgetCommand(budget.Id, "Quarterly", 2000m, "USD"), CancellationToken.None);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Validator_RejectsEmptyBudgetId()
    {
        var validator = new UpdateBudgetCommandValidator();
        var result = validator.Validate(new UpdateBudgetCommand(Guid.Empty, "Name", 100m, "EUR"));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateBudgetCommand.BudgetId));
    }

    [Fact]
    public void Validator_RejectsEmptyName()
    {
        var validator = new UpdateBudgetCommandValidator();
        var result = validator.Validate(new UpdateBudgetCommand(Guid.NewGuid(), "", 100m, "EUR"));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateBudgetCommand.Name));
    }

    [Fact]
    public void Validator_RejectsZeroLimit()
    {
        var validator = new UpdateBudgetCommandValidator();
        var result = validator.Validate(new UpdateBudgetCommand(Guid.NewGuid(), "Name", 0m, "EUR"));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateBudgetCommand.Limit));
    }

    [Theory]
    [InlineData("EU")]
    [InlineData("EURO")]
    public void Validator_RejectsCurrencyNotThreeChars(string currency)
    {
        var validator = new UpdateBudgetCommandValidator();
        var result = validator.Validate(new UpdateBudgetCommand(Guid.NewGuid(), "Name", 100m, currency));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateBudgetCommand.Currency));
    }
}
