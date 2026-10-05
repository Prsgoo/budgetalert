using BudgetAlert.Api.Models;
using BudgetAlert.Application.Budgets.Commands;
using BudgetAlert.Application.Budgets.Queries;
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
            return CreatedAtAction(nameof(GetById), new { budgetId, ruleId = id }, null);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromRoute] Guid budgetId, CancellationToken cancellationToken)
        {
            var rules = await _sender.Send(new GetAlertRulesQuery(budgetId), cancellationToken);
            return Ok(rules);
        }

        [HttpGet("{ruleId:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid budgetId, [FromRoute] Guid ruleId, CancellationToken cancellationToken)
        {
            var rule = await _sender.Send(new GetAlertRuleByIdQuery(budgetId, ruleId), cancellationToken);
            return Ok(rule);
        }

        [HttpPatch("{ruleId:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid budgetId, [FromRoute] Guid ruleId, [FromBody] UpdateAlertRuleRequest request, CancellationToken cancellationToken)
        {
            await _sender.Send(new UpdateAlertRuleCommand(budgetId, ruleId, request.ThresholdPercentage, request.IsActive), cancellationToken);
            return NoContent();
        }
    }
}
