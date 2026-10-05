using BudgetAlert.Domain.Entities;

namespace BudgetAlert.Domain.Tests;

public class AlertRuleTests
{
    [Fact]
    public void Create_SetsThresholdPercentage()
    {
        var rule = AlertRule.Create(Guid.NewGuid(), 80m);
        Assert.Equal(80m, rule.ThresholdPercentage);
    }

    [Fact]
    public void Create_SetsBudgetId()
    {
        var budgetId = Guid.NewGuid();
        var rule = AlertRule.Create(budgetId, 80m);
        Assert.Equal(budgetId, rule.BudgetId);
    }

    [Fact]
    public void Create_GeneratesNonEmptyId()
    {
        var rule = AlertRule.Create(Guid.NewGuid(), 80m);
        Assert.NotEqual(Guid.Empty, rule.Id);
    }

    [Fact]
    public void Create_SetsIsActiveToTrue()
    {
        var rule = AlertRule.Create(Guid.NewGuid(), 80m);
        Assert.True(rule.IsActive);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(0.9)]
    public void Create_WhenThresholdBelowOne_ThrowsArgumentOutOfRangeException(decimal threshold)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AlertRule.Create(Guid.NewGuid(), threshold));
    }

    [Theory]
    [InlineData(101)]
    [InlineData(200)]
    public void Create_WhenThresholdAbove100_ThrowsArgumentOutOfRangeException(decimal threshold)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AlertRule.Create(Guid.NewGuid(), threshold));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(50)]
    public void Create_WithBoundaryThresholds_Succeeds(decimal threshold)
    {
        var rule = AlertRule.Create(Guid.NewGuid(), threshold);
        Assert.Equal(threshold, rule.ThresholdPercentage);
    }

    [Fact]
    public void Update_ChangesThresholdAndIsActive()
    {
        var rule = AlertRule.Create(Guid.NewGuid(), 80m);
        rule.Update(50m, false);
        Assert.Equal(50m, rule.ThresholdPercentage);
        Assert.False(rule.IsActive);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Update_WithInvalidThreshold_ThrowsArgumentOutOfRangeException(decimal threshold)
    {
        var rule = AlertRule.Create(Guid.NewGuid(), 80m);
        Assert.Throws<ArgumentOutOfRangeException>(() => rule.Update(threshold, true));
    }
}
