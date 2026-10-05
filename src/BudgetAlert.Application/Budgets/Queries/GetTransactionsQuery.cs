using BudgetAlert.Application.Budgets.DTOs;
using BudgetAlert.Application.Exceptions;
using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Repositories;
using MediatR;

namespace BudgetAlert.Application.Budgets.Queries
{
    public record GetTransactionsQuery(Guid BudgetId) : IRequest<IReadOnlyList<TransactionDto>>;

    public class GetTransactionsQueryHandler(IBudgetRepository _budgetRepository) : IRequestHandler<GetTransactionsQuery, IReadOnlyList<TransactionDto>>
    {
        public async Task<IReadOnlyList<TransactionDto>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
        {
            var budget = await _budgetRepository.GetByIdWithTransactionsAsync(request.BudgetId, cancellationToken)
                ?? throw new NotFoundException(nameof(Budget), request.BudgetId);
            return budget.Transactions
                .OrderByDescending(t => t.OccurredAt)
                .Select(t => new TransactionDto(t.Id, t.Amount, t.Description, t.OccurredAt))
                .ToList();
        }
    }
}
