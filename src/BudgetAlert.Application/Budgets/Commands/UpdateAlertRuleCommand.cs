using BudgetAlert.Application.Exceptions;
using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace BudgetAlert.Application.Budgets.Commands
{
    public record UpdateAlertRuleCommand(Guid BudgetId, Guid AlertRuleId, decimal ThresholdPercentage, bool IsActive) : IRequest;

    public class UpdateAlertRuleCommandValidator : AbstractValidator<UpdateAlertRuleCommand>
    {
        public UpdateAlertRuleCommandValidator()
        {
            RuleFor(x => x.BudgetId).NotEmpty();
            RuleFor(x => x.AlertRuleId).NotEmpty();
            RuleFor(x => x.ThresholdPercentage).InclusiveBetween(1, 100);
        }
    }

    public class UpdateAlertRuleCommandHandler(IBudgetRepository _budgetRepository, IUnitOfWork _unitOfWork) : IRequestHandler<UpdateAlertRuleCommand>
    {
        public async Task Handle(UpdateAlertRuleCommand request, CancellationToken cancellationToken)
        {
            var budget = await _budgetRepository.GetByIdWithAlertRulesAsync(request.BudgetId, cancellationToken)
                ?? throw new NotFoundException(nameof(Budget), request.BudgetId);
            var rule = budget.AlertRules.SingleOrDefault(r => r.Id == request.AlertRuleId)
                ?? throw new NotFoundException(nameof(AlertRule), request.AlertRuleId);
            rule.Update(request.ThresholdPercentage, request.IsActive);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
