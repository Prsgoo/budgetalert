namespace BudgetAlert.Application.Budgets.DTOs
{
    public record AlertRuleDto(Guid Id, Guid BudgetId, decimal ThresholdPercentage, bool IsActive);
}
