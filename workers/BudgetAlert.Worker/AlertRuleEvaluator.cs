using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Events;
using BudgetAlert.Domain.Repositories;

namespace BudgetAlert.Worker
{
    public class AlertRuleEvaluator(IBudgetRepository _budgetRepo, IAlertRepository _alertRepo, IUnitOfWork _uow)
    {

        public async Task EvaluateAsync(TransactionRegistered evt, CancellationToken ct)
        {
            var budget = await _budgetRepo.GetByIdWithAlertRulesAsync(evt.BudgetId, ct);
            if (budget is null) return;

            foreach (var rule in budget.AlertRules.Where(r => r.IsActive))
            {
                var threshold = budget.Limit * (rule.ThresholdPercentage / 100m);
                var wasUnderThreshold = (evt.CurrentSpend - evt.Amount) < threshold;
                var isOverThreshold = evt.CurrentSpend >= threshold;

                if (wasUnderThreshold && isOverThreshold)
                {
                    var alert = Alert.Create(budget.Id, rule.Id, evt.CurrentSpend, rule.ThresholdPercentage, budget.Limit);
                    _alertRepo.Add(alert);
                }
            }

            await _uow.SaveChangesAsync(ct);
        }
    }
}