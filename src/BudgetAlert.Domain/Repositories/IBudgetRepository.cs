using BudgetAlert.Domain.Entities;

namespace BudgetAlert.Domain.Repositories
{
    public interface IBudgetRepository
    {
        Task<Budget?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Budget?> GetByIdWithTransactionsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Budget?> GetByIdWithAlertRulesAsync(Guid id, CancellationToken cancellationToken = default);
        void Add(Budget budget);
        void AddAlertRule(AlertRule alertRule);
        void AddTransaction(Transaction transaction);
    }
}