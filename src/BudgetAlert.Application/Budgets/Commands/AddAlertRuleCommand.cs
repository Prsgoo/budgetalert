using BudgetAlert.Application.Exceptions;
using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace BudgetAlert.Application.Budgets.Commands
{
    public record AddAlertRuleCommand(Guid BudgetId, decimal ThresholdPercentage) : IRequest<Guid>;

    public class AddAlertRuleCommandValidator : AbstractValidator<AddAlertRuleCommand>
    {
        public AddAlertRuleCommandValidator()
        {
            RuleFor(x => x.BudgetId).NotEmpty();
            RuleFor(x => x.ThresholdPercentage).InclusiveBetween(1, 100);
        }
    }

    public class AddAlertRuleCommandHandler(IBudgetRepository _budgetRepository, IUnitOfWork _unitOfWork) : IRequestHandler<AddAlertRuleCommand, Guid>
    {
        public async Task<Guid> Handle(AddAlertRuleCommand request, CancellationToken cancellationToken)
        {
            var budget = await _budgetRepository.GetByIdAsync(request.BudgetId, cancellationToken) ?? throw new NotFoundException(nameof(Budget), request.BudgetId);
            var alertRule = budget.AddAlertRule(request.ThresholdPercentage);
            _budgetRepository.AddAlertRule(alertRule);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return alertRule.Id;
        }
    }
}