namespace BudgetAlert.Api.Models
{
    public record CreateBudgetRequest(string Name, decimal Limit, string Currency);
    public record AddAlertRuleRequest(decimal ThresholdPercentage);
}