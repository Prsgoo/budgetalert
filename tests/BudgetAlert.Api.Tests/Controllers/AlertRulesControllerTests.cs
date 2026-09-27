using System.Net;
using System.Net.Http.Json;
using BudgetAlert.Api.Tests.Helpers;

namespace BudgetAlert.Api.Tests.Controllers;

public class AlertRulesControllerTests(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<Guid> CreateBudgetAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/budgets", new
        {
            name = "Budget",
            limit = 1000m,
            currency = "EUR"
        });
        var id = response.Headers.Location!.Segments.Last();
        return Guid.Parse(id);
    }

    [Fact]
    public async Task AddAlertRule_WithValidData_Returns201()
    {
        var budgetId = await CreateBudgetAsync();

        var response = await _client.PostAsJsonAsync($"/api/budgets/{budgetId}/rules", new
        {
            thresholdPercentage = 80m
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task AddAlertRule_ForNonexistentBudget_Returns404()
    {
        var response = await _client.PostAsJsonAsync($"/api/budgets/{Guid.NewGuid()}/rules", new
        {
            thresholdPercentage = 80m
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddAlertRule_WithThresholdAbove100_Returns400()
    {
        var response = await _client.PostAsJsonAsync($"/api/budgets/{Guid.NewGuid()}/rules", new
        {
            thresholdPercentage = 150m
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddAlertRule_WithZeroThreshold_Returns400()
    {
        var response = await _client.PostAsJsonAsync($"/api/budgets/{Guid.NewGuid()}/rules", new
        {
            thresholdPercentage = 0m
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
