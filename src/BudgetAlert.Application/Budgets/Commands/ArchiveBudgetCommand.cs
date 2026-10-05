using BudgetAlert.Application.Exceptions;
using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Repositories;
using MediatR;

namespace BudgetAlert.Application.Budgets.Commands
{
    public record ArchiveBudgetCommand(Guid BudgetId) : IRequest;

    public class ArchiveBudgetCommandHandler(IBudgetRepository _budgetRepository, IUnitOfWork _unitOfWork) : IRequestHandler<ArchiveBudgetCommand>
    {
        public async Task Handle(ArchiveBudgetCommand request, CancellationToken cancellationToken)
        {
            var budget = await _budgetRepository.GetByIdAsync(request.BudgetId, cancellationToken)
                ?? throw new NotFoundException(nameof(Budget), request.BudgetId);
            budget.Archive();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
