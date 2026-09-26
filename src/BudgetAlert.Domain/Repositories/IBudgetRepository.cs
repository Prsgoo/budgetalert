using BudgetAlert.Domain.Entities;

namespace BudgetAlert.Domain.Repositories
{
    // BudgetAlert.Domain/Repositories/IBudgetRepository.cs
    public interface IBudgetRepository
    {
        Task<Budget?> GetByIdAsync(Guid id, CancellationToken ct = default);
        void Add(Budget budget);
        Task<Budget?> GetByIdWithTransactionsAsync(Guid id, CancellationToken ct = default);
        Task<Budget?> GetByIdWithAlertRulesAsync(Guid id, CancellationToken ct = default);
    }
}