using BudgetAlert.Application.Budgets.DTOs;
using BudgetAlert.Application.Exceptions;
using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Repositories;
using MediatR;

namespace BudgetAlert.Application.Budgets.Queries
{
    public record GetAlertRulesQuery(Guid BudgetId) : IRequest<IReadOnlyList<AlertRuleDto>>;

    public class GetAlertRulesQueryHandler(IBudgetRepository _budgetRepository) : IRequestHandler<GetAlertRulesQuery, IReadOnlyList<AlertRuleDto>>
    {
        public async Task<IReadOnlyList<AlertRuleDto>> Handle(GetAlertRulesQuery request, CancellationToken cancellationToken)
        {
            var budget = await _budgetRepository.GetByIdWithAlertRulesAsync(request.BudgetId, cancellationToken)
                ?? throw new NotFoundException(nameof(Budget), request.BudgetId);
            return budget.AlertRules
                .Select(r => new AlertRuleDto(r.Id, r.BudgetId, r.ThresholdPercentage, r.IsActive))
                .ToList();
        }
    }
}
