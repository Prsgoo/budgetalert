using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BudgetAlert.Infrastructure.Persistence.Repositories
{
    public class BudgetRepository(BudgetAlertDbContext _context) : IBudgetRepository
    {
        public void Add(Budget budget)
        {
            _context.Budgets.Add(budget);
        }

        public Task<Budget?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _context.Budgets.FindAsync(id, cancellationToken).AsTask();
        }

        public Task<Budget?> GetByIdWithTransactionsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _context.Budgets
                .Include(b => b.Transactions)
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public Task<Budget?> GetByIdWithAlertRulesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _context.Budgets
                .Include(b => b.AlertRules)
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }
    }
}
