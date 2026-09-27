using BudgetAlert.Application.Alerts.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BudgetAlert.Api.Controllers
{
    [ApiController]
    [Route("api/alerts")]
    public class AlertsController(ISender _sender) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAlerts([FromQuery] Guid? budgetId, CancellationToken cancellationToken)
        {
            var alerts = await _sender.Send(new GetAlertsQuery(budgetId), cancellationToken);
            return Ok(alerts);
        }
    }
}