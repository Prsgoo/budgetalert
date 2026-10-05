using BudgetAlert.Application.Contracts;
using BudgetAlert.Application.Exceptions;
using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace BudgetAlert.Application.Budgets.Commands
{
    public record RegisterTransactionCommand(
    Guid BudgetId,
    decimal Amount,
    string Description,
    DateTime OccurredAt
) : IRequest<Guid>;

    public class RegisterTransactionCommandValidator : AbstractValidator<RegisterTransactionCommand>
    {
        public RegisterTransactionCommandValidator()
        {
            RuleFor(x => x.BudgetId).NotEmpty();
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
            // Lambda so DateTime.UtcNow is evaluated at validation time, not when the validator is constructed.
            // Validators are singletons - a static value here would be frozen at app startup.
            RuleFor(x => x.OccurredAt).LessThanOrEqualTo(_ => DateTime.UtcNow);
        }
    }

    public class RegisterTransactionCommandHandler(IBudgetRepository _budgetRepository, IUnitOfWork _unitOfWork, IEventBus _eventBus) : IRequestHandler<RegisterTransactionCommand, Guid>
    {

        public async Task<Guid> Handle(RegisterTransactionCommand request, CancellationToken cancellationToken)
        {
            var budget = await _budgetRepository.GetByIdWithTransactionsAsync(request.BudgetId, cancellationToken)
                ?? throw new NotFoundException(nameof(Budget), request.BudgetId);

            var transaction = budget.RegisterTransaction(request.Amount, request.Description, request.OccurredAt);
            _budgetRepository.AddTransaction(transaction);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Events are published after SaveChangesAsync so we never publish an event for a transaction
            // that failed to persist. The trade-off is at-most-once delivery - if the broker call fails,
            // the transaction is saved but the event is lost. An outbox pattern would give at-least-once.
            foreach (var evt in budget.PopDomainEvents())
                await _eventBus.PublishAsync(evt, cancellationToken);

            return transaction.Id;
        }
    }
}