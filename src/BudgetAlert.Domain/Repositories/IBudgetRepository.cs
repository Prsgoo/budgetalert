using BudgetAlert.Domain.Entities;

namespace BudgetAlert.Domain.Repositories
{
    // BudgetAlert.Domain/Repositories/IBudgetRepository.cs
    public interface IBudgetRepository
    {
        Task<Budget?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        void Add(Budget budget);
        Task<Budget?> GetByIdWithTransactionsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Budget?> GetByIdWithAlertRulesAsync(Guid id, CancellationToken cancellationToken = default);
    }
}