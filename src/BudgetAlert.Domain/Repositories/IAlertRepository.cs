using BudgetAlert.Domain.Entities;

namespace BudgetAlert.Domain.Repositories
{
    public interface IAlertRepository
    {
        Task<IReadOnlyList<Alert>> GetAsync(Guid? budgetId, CancellationToken cancellationToken = default);
        void Add(Alert alert);
    }
}