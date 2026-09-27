using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Events;
using BudgetAlert.Domain.Repositories;
using Moq;

namespace BudgetAlert.Worker.Tests;

public class AlertRuleEvaluatorTests
{
    private readonly Mock<IBudgetRepository> _budgetRepo = new();
    private readonly Mock<IAlertRepository> _alertRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private AlertRuleEvaluator CreateEvaluator() =>
        new(_budgetRepo.Object, _alertRepo.Object, _unitOfWork.Object);

    private static TransactionRegistered CreateEvent(Guid budgetId, decimal amount, decimal currentSpend, decimal budgetLimit = 1000m) =>
        new(Guid.NewGuid(), DateTime.UtcNow, budgetId, Guid.NewGuid(), amount, currentSpend, budgetLimit);

    [Fact]
    public async Task EvaluateAsync_DoesNothingWhenBudgetNotFound()
    {
        _budgetRepo.Setup(r => r.GetByIdWithAlertRulesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Budget?)null);
        var evaluator = CreateEvaluator();

        await evaluator.EvaluateAsync(CreateEvent(Guid.NewGuid(), 100m, 100m), CancellationToken.None);

        _alertRepo.Verify(r => r.Add(It.IsAny<Alert>()), Times.Never);
    }

    [Fact]
    public async Task EvaluateAsync_CreatesAlertWhenSpendCrossesThresholdForFirstTime()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        budget.AddAlertRule(80m);
        _budgetRepo.Setup(r => r.GetByIdWithAlertRulesAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var evaluator = CreateEvaluator();

        // currentSpend = 850 (over 80% = 800), previous spend = 750 (under threshold)
        await evaluator.EvaluateAsync(CreateEvent(budget.Id, 100m, 850m), CancellationToken.None);

        _alertRepo.Verify(r => r.Add(It.IsAny<Alert>()), Times.Once);
    }

    [Fact]
    public async Task EvaluateAsync_DoesNotCreateAlertWhenSpendWasAlreadyAboveThreshold()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        budget.AddAlertRule(80m);
        _budgetRepo.Setup(r => r.GetByIdWithAlertRulesAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var evaluator = CreateEvaluator();

        // currentSpend = 950, previous spend = 850 - both above 80% threshold, no crossing
        await evaluator.EvaluateAsync(CreateEvent(budget.Id, 100m, 950m), CancellationToken.None);

        _alertRepo.Verify(r => r.Add(It.IsAny<Alert>()), Times.Never);
    }

    [Fact]
    public async Task EvaluateAsync_SkipsInactiveRules()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        budget.AddAlertRule(80m);
        var rule = budget.AlertRules.Single();
        // IsActive has a private setter - use reflection to deactivate for this test
        typeof(AlertRule).GetProperty(nameof(AlertRule.IsActive))!
            .SetValue(rule, false);
        _budgetRepo.Setup(r => r.GetByIdWithAlertRulesAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var evaluator = CreateEvaluator();

        // spend crosses threshold but rule is inactive - no alert should be created
        await evaluator.EvaluateAsync(CreateEvent(budget.Id, 100m, 850m), CancellationToken.None);

        _alertRepo.Verify(r => r.Add(It.IsAny<Alert>()), Times.Never);
    }

    [Fact]
    public async Task EvaluateAsync_DoesNotCreateAlertWhenSpendBelowThreshold()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        budget.AddAlertRule(80m);
        _budgetRepo.Setup(r => r.GetByIdWithAlertRulesAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var evaluator = CreateEvaluator();

        // currentSpend = 500, previous = 400 - both below 80% (800) threshold
        await evaluator.EvaluateAsync(CreateEvent(budget.Id, 100m, 500m), CancellationToken.None);

        _alertRepo.Verify(r => r.Add(It.IsAny<Alert>()), Times.Never);
    }

    [Fact]
    public async Task EvaluateAsync_CreatesAlertForEachCrossingRule()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        budget.AddAlertRule(50m);
        budget.AddAlertRule(80m);
        _budgetRepo.Setup(r => r.GetByIdWithAlertRulesAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var evaluator = CreateEvaluator();

        // currentSpend = 900, previous = 400 - crosses both 50% (500) and 80% (800)
        await evaluator.EvaluateAsync(CreateEvent(budget.Id, 500m, 900m), CancellationToken.None);

        _alertRepo.Verify(r => r.Add(It.IsAny<Alert>()), Times.Exactly(2));
    }

    [Fact]
    public async Task EvaluateAsync_OnlyCreatesAlertForRuleThatCrossesThreshold()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        budget.AddAlertRule(80m);
        budget.AddAlertRule(95m);
        _budgetRepo.Setup(r => r.GetByIdWithAlertRulesAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        var evaluator = CreateEvaluator();

        // currentSpend = 850, previous = 750; crosses 80% (800) but not 95% (950)
        await evaluator.EvaluateAsync(CreateEvent(budget.Id, 100m, 850m), CancellationToken.None);

        _alertRepo.Verify(r => r.Add(It.IsAny<Alert>()), Times.Once);
    }

    [Fact]
    public async Task EvaluateAsync_CallsSaveChangesAsync()
    {
        var budget = Budget.Create("Monthly", 1000m, "EUR");
        _budgetRepo.Setup(r => r.GetByIdWithAlertRulesAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);
        var evaluator = CreateEvaluator();

        await evaluator.EvaluateAsync(CreateEvent(budget.Id, 0m, 0m), CancellationToken.None);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
