using BudgetAlert.Application.Budgets.Commands;
using BudgetAlert.Application.Contracts;
using BudgetAlert.Application.Exceptions;
using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Events;
using BudgetAlert.Domain.Repositories;
using Moq;

namespace BudgetAlert.Application.Tests.Budgets.Commands;

public class RegisterTransactionCommandHandlerTests
{
    private readonly Mock<IBudgetRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IEventBus> _eventBus = new();

    [Fact]
    public async Task Handle_ThrowsNotFoundExceptionWhenBudgetNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Budget?)null);
        var handler = new RegisterTransactionCommandHandler(_repo.Object, _unitOfWork.Object, _eventBus.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new RegisterTransactionCommand(Guid.NewGuid(), 100m, "Groceries", DateTime.UtcNow), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ReturnsNonEmptyTransactionId()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        _repo.Setup(r => r.GetByIdAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new RegisterTransactionCommandHandler(_repo.Object, _unitOfWork.Object, _eventBus.Object);

        var id = await handler.Handle(
            new RegisterTransactionCommand(budget.Id, 100m, "Groceries", DateTime.UtcNow), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task Handle_PublishesEventAfterSave()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        _repo.Setup(r => r.GetByIdAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new RegisterTransactionCommandHandler(_repo.Object, _unitOfWork.Object, _eventBus.Object);

        await handler.Handle(
            new RegisterTransactionCommand(budget.Id, 100m, "Groceries", DateTime.UtcNow), CancellationToken.None);

        _eventBus.Verify(e => e.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DoesNotPublishEventWhenSaveFails()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        _repo.Setup(r => r.GetByIdAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));
        var handler = new RegisterTransactionCommandHandler(_repo.Object, _unitOfWork.Object, _eventBus.Object);

        await Assert.ThrowsAsync<Exception>(() =>
            handler.Handle(new RegisterTransactionCommand(budget.Id, 100m, "Groceries", DateTime.UtcNow), CancellationToken.None));

        _eventBus.Verify(e => e.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Validator_RejectsFutureOccurredAt()
    {
        var validator = new RegisterTransactionCommandValidator();
        var result = validator.Validate(
            new RegisterTransactionCommand(Guid.NewGuid(), 100m, "Groceries", DateTime.UtcNow.AddDays(1)));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterTransactionCommand.OccurredAt));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validator_RejectsAmountNotGreaterThanZero(decimal amount)
    {
        var validator = new RegisterTransactionCommandValidator();
        var result = validator.Validate(
            new RegisterTransactionCommand(Guid.NewGuid(), amount, "Groceries", DateTime.UtcNow));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterTransactionCommand.Amount));
    }

    [Fact]
    public void Validator_RejectsEmptyDescription()
    {
        var validator = new RegisterTransactionCommandValidator();
        var result = validator.Validate(
            new RegisterTransactionCommand(Guid.NewGuid(), 100m, "", DateTime.UtcNow));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterTransactionCommand.Description));
    }
}
