using BudgetAlert.Domain.Events;

namespace BudgetAlert.Domain.Tests.Events;

public class AlertTriggeredTests
{
    [Fact]
    public void AlertTriggered_StoresAllProperties()
    {
        var eventId = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow;
        var budgetId = Guid.NewGuid();
        var alertRuleId = Guid.NewGuid();

        var evt = new AlertTriggered(eventId, occurredAt, budgetId, alertRuleId, 80m, 850m, 1000m);

        Assert.Equal(eventId, evt.EventId);
        Assert.Equal(occurredAt, evt.OccurredAt);
        Assert.Equal(budgetId, evt.BudgetId);
        Assert.Equal(alertRuleId, evt.AlertRuleId);
        Assert.Equal(80m, evt.ThresholdPercentage);
        Assert.Equal(850m, evt.SpendAtTrigger);
        Assert.Equal(1000m, evt.BudgetLimit);
    }
}
