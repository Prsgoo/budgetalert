using BudgetAlert.Api.Models;
using BudgetAlert.Application.Budgets.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BudgetAlert.Api.Controllers
{
    [ApiController]
    [Route("api/budgets/{budgetId:guid}/rules")]
    public class AlertRulesController(ISender _sender) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Add([FromRoute] Guid budgetId, [FromBody] AddAlertRuleRequest request, CancellationToken cancellationToken)
        {
            var id = await _sender.Send(new AddAlertRuleCommand(budgetId, request.ThresholdPercentage), cancellationToken);
            return StatusCode(201, id);
        }
    }
}
