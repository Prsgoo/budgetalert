using BudgetAlert.Application.Exceptions;
using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace BudgetAlert.Application.Budgets.Commands
{
    public record UpdateBudgetCommand(Guid BudgetId, string Name, decimal Limit, string Currency) : IRequest;

    public class UpdateBudgetCommandValidator : AbstractValidator<UpdateBudgetCommand>
    {
        public UpdateBudgetCommandValidator()
        {
            RuleFor(x => x.BudgetId).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Limit).GreaterThan(0);
            RuleFor(x => x.Currency).NotEmpty().Length(3);
        }
    }

    public class UpdateBudgetCommandHandler(IBudgetRepository _budgetRepository, IUnitOfWork _unitOfWork) : IRequestHandler<UpdateBudgetCommand>
    {
        public async Task Handle(UpdateBudgetCommand request, CancellationToken cancellationToken)
        {
            var budget = await _budgetRepository.GetByIdAsync(request.BudgetId, cancellationToken)
                ?? throw new NotFoundException(nameof(Budget), request.BudgetId);
            if (budget.IsArchived)
                throw new NotFoundException(nameof(Budget), request.BudgetId);
            budget.UpdateDetails(request.Name, request.Limit, request.Currency);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
