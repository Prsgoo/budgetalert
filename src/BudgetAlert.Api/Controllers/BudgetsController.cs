using BudgetAlert.Api.Models;
using BudgetAlert.Application.Budgets.Commands;
using BudgetAlert.Application.Budgets.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BudgetAlert.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetsController(ISender _sender) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBudgetRequest request, CancellationToken cancellationToken)
        {
            var id = await _sender.Send(new CreateBudgetCommand(request.Name, request.Limit, request.Currency), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id }, null);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var budget = await _sender.Send(new GetBudgetQuery(id), cancellationToken);
            return Ok(budget);
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBudgetRequest request, CancellationToken cancellationToken)
        {
            await _sender.Send(new UpdateBudgetCommand(id, request.Name, request.Limit, request.Currency), cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
        {
            await _sender.Send(new ArchiveBudgetCommand(id), cancellationToken);
            return NoContent();
        }
    }
}
