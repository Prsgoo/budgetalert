using BudgetAlert.Application.Budgets.Commands;
using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Repositories;
using Moq;

namespace BudgetAlert.Application.Tests.Budgets.Commands;

public class CreateBudgetCommandHandlerTests
{
    private readonly Mock<IBudgetRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Handle_CallsRepositoryAdd()
    {
        var handler = new CreateBudgetCommandHandler(_repo.Object, _unitOfWork.Object);

        await handler.Handle(new CreateBudgetCommand("Monthly", 1000m, "EUR"), CancellationToken.None);

        _repo.Verify(r => r.Add(It.IsAny<Budget>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CallsSaveChanges()
    {
        var handler = new CreateBudgetCommandHandler(_repo.Object, _unitOfWork.Object);

        await handler.Handle(new CreateBudgetCommand("Monthly", 1000m, "EUR"), CancellationToken.None);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsNonEmptyGuid()
    {
        var handler = new CreateBudgetCommandHandler(_repo.Object, _unitOfWork.Object);

        var id = await handler.Handle(new CreateBudgetCommand("Monthly", 1000m, "EUR"), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public void Validator_RejectsEmptyName()
    {
        var validator = new CreateBudgetCommandValidator();
        var result = validator.Validate(new CreateBudgetCommand("", 1000m, "EUR"));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBudgetCommand.Name));
    }

    [Fact]
    public void Validator_RejectsNameExceeding100Chars()
    {
        var validator = new CreateBudgetCommandValidator();
        var result = validator.Validate(new CreateBudgetCommand(new string('a', 101), 1000m, "EUR"));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBudgetCommand.Name));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validator_RejectsLimitNotGreaterThanZero(decimal limit)
    {
        var validator = new CreateBudgetCommandValidator();
        var result = validator.Validate(new CreateBudgetCommand("Monthly", limit, "EUR"));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBudgetCommand.Limit));
    }

    [Fact]
    public void Validator_RejectsCurrencyWithWrongLength()
    {
        var validator = new CreateBudgetCommandValidator();
        var result = validator.Validate(new CreateBudgetCommand("Monthly", 1000m, "EU"));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBudgetCommand.Currency));
    }

    [Fact]
    public void Validator_RejectsEmptyCurrency()
    {
        var validator = new CreateBudgetCommandValidator();
        var result = validator.Validate(new CreateBudgetCommand("Monthly", 1000m, ""));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateBudgetCommand.Currency));
    }

    [Fact]
    public void Validator_AcceptsValidCommand()
    {
        var validator = new CreateBudgetCommandValidator();
        var result = validator.Validate(new CreateBudgetCommand("Monthly", 1000m, "EUR"));
        Assert.True(result.IsValid);
    }
}
