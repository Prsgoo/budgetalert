using BudgetAlert.Application.Budgets.Commands;
using BudgetAlert.Application.Exceptions;
using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Repositories;
using Moq;

namespace BudgetAlert.Application.Tests.Budgets.Commands;

public class ArchiveBudgetCommandHandlerTests
{
    private readonly Mock<IBudgetRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Handle_ThrowsNotFoundExceptionWhenBudgetNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Budget?)null);
        var handler = new ArchiveBudgetCommandHandler(_repo.Object, _unitOfWork.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new ArchiveBudgetCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SetsBudgetAsArchived()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        _repo.Setup(r => r.GetByIdAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new ArchiveBudgetCommandHandler(_repo.Object, _unitOfWork.Object);

        await handler.Handle(new ArchiveBudgetCommand(budget.Id), CancellationToken.None);

        Assert.True(budget.IsArchived);
    }

    [Fact]
    public async Task Handle_SavesChanges()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        _repo.Setup(r => r.GetByIdAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new ArchiveBudgetCommandHandler(_repo.Object, _unitOfWork.Object);

        await handler.Handle(new ArchiveBudgetCommand(budget.Id), CancellationToken.None);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_IsIdempotentWhenAlreadyArchived()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        budget.Archive();
        _repo.Setup(r => r.GetByIdAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var handler = new ArchiveBudgetCommandHandler(_repo.Object, _unitOfWork.Object);

        await handler.Handle(new ArchiveBudgetCommand(budget.Id), CancellationToken.None);

        Assert.True(budget.IsArchived);
    }
}
