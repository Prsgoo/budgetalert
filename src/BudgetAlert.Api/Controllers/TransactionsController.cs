using BudgetAlert.Api.Models;
using BudgetAlert.Application.Budgets.Commands;
using BudgetAlert.Application.Budgets.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BudgetAlert.Api.Controllers
{
    [ApiController]
    [Route("api/budgets/{budgetId:guid}/transactions")]
    public class TransactionsController(ISender _sender) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> RegisterTransaction([FromRoute] Guid budgetId, [FromBody] RegisterTransactionRequest request, CancellationToken cancellationToken)
        {
            var transactionId = await _sender.Send(
                new RegisterTransactionCommand(budgetId, request.Amount, request.Description, request.OccurredAt ?? DateTime.UtcNow),
                cancellationToken);

            return CreatedAtAction(nameof(BudgetsController.GetById), "Budgets", new { id = budgetId }, new { transactionId });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromRoute] Guid budgetId, CancellationToken cancellationToken)
        {
            var transactions = await _sender.Send(new GetTransactionsQuery(budgetId), cancellationToken);
            return Ok(transactions);
        }
    }
}
