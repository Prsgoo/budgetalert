using BudgetAlert.Domain.Events;
using BudgetAlert.Domain.Repositories;

namespace BudgetAlert.Worker
{
    public class AlertRuleEvaluator(IBudgetRepository _budgetRepo, IAlertRepository _alertRepo, IUnitOfWork _unitOfWork)
    {
        public async Task EvaluateAsync(TransactionRegistered evt, CancellationToken cancellationToken)
        {
            var budget = await _budgetRepo.GetByIdWithAlertRulesAsync(evt.BudgetId, cancellationToken);
            if (budget is null) return;

            foreach (var alert in budget.EvaluateAlertRules(evt.CurrentSpend - evt.Amount, evt.CurrentSpend))
                _alertRepo.Add(alert);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}