namespace BudgetAlert.Api.Models
{
    public record RegisterTransactionRequest(decimal Amount, string Description, DateTime? OccurredAt);
}
