namespace BudgetAlert.Api.Models
{
    public record CreateBudgetRequest(string Name, decimal Limit, string Currency);
    public record UpdateBudgetRequest(string Name, decimal Limit, string Currency);
    public record AddAlertRuleRequest(decimal ThresholdPercentage);
    public record UpdateAlertRuleRequest(decimal ThresholdPercentage, bool IsActive);
}