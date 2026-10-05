using BudgetAlert.Application.Budgets.DTOs;
using BudgetAlert.Application.Exceptions;
using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Repositories;
using MediatR;

namespace BudgetAlert.Application.Budgets.Queries
{
    public record GetBudgetQuery(Guid BudgetId) : IRequest<BudgetDto>;

    public class GetBudgetQueryHandler(IBudgetRepository _budgetRepository) : IRequestHandler<GetBudgetQuery, BudgetDto>
    {
        public async Task<BudgetDto> Handle(GetBudgetQuery request, CancellationToken cancellationToken)
        {
            var budget = await _budgetRepository.GetByIdWithTransactionsAsync(request.BudgetId, cancellationToken)
                ?? throw new NotFoundException(nameof(Budget), request.BudgetId);
            if (budget.IsArchived)
                throw new NotFoundException(nameof(Budget), request.BudgetId);
            
            var recentTransactions = budget.Transactions
                .OrderByDescending(t => t.OccurredAt)
                .Take(10)
                .Select(t => new TransactionDto(t.Id, t.Amount, t.Description, t.OccurredAt))
                .ToList();

            return new BudgetDto(
                budget.Id,
                budget.Name,
                budget.Limit,
                budget.Currency,
                budget.CurrentSpend,
                budget.SpendPercentage,
                budget.CreatedAt,
                recentTransactions
            );
        }
    }
}