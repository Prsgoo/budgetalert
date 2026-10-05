using BudgetAlert.Application.Budgets.DTOs;
using BudgetAlert.Application.Exceptions;
using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Repositories;
using MediatR;

namespace BudgetAlert.Application.Budgets.Queries
{
    public record GetAlertRuleByIdQuery(Guid BudgetId, Guid AlertRuleId) : IRequest<AlertRuleDto>;

    public class GetAlertRuleByIdQueryHandler(IBudgetRepository _budgetRepository) : IRequestHandler<GetAlertRuleByIdQuery, AlertRuleDto>
    {
        public async Task<AlertRuleDto> Handle(GetAlertRuleByIdQuery request, CancellationToken cancellationToken)
        {
            var budget = await _budgetRepository.GetByIdWithAlertRulesAsync(request.BudgetId, cancellationToken)
                ?? throw new NotFoundException(nameof(Budget), request.BudgetId);
            var rule = budget.AlertRules.SingleOrDefault(r => r.Id == request.AlertRuleId)
                ?? throw new NotFoundException(nameof(AlertRule), request.AlertRuleId);
            return new AlertRuleDto(rule.Id, rule.BudgetId, rule.ThresholdPercentage, rule.IsActive);
        }
    }
}
