using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BudgetAlert.Infrastructure.Persistence.Repositories
{
    public class AlertRepository(BudgetAlertDbContext _context) : IAlertRepository
    {
        public void Add(Alert alert)
        {
            _context.Alerts.Add(alert);
        }

        public async Task<IReadOnlyList<Alert>> GetAsync(Guid? budgetId, CancellationToken ct = default)
        {
            var query = _context.Alerts.AsQueryable();

            if (budgetId.HasValue)
            {
                query = query.Where(a => a.BudgetId == budgetId.Value);
            }

            return await query.ToListAsync(ct);
        }
    }
}