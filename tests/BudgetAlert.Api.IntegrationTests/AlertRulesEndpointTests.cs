using System.Net;
using System.Net.Http.Json;
using BudgetAlert.Api.IntegrationTests.Helpers;
using FluentAssertions;

namespace BudgetAlert.Api.IntegrationTests;

public class AlertRulesEndpointTests(BudgetAlertApiFactory factory) : IClassFixture<BudgetAlertApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<Guid> CreateBudgetAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/budgets", new { name = "Budget", limit = 1000m, currency = "EUR" });
        response.EnsureSuccessStatusCode();
        return Guid.Parse(response.Headers.Location!.Segments.Last());
    }

    [Fact]
    public async Task AddAlertRule_ShouldReturn201WithRuleId()
    {
        var budgetId = await CreateBudgetAsync();

        var response = await _client.PostAsJsonAsync($"/api/budgets/{budgetId}/rules", new
        {
            thresholdPercentage = 80m
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var ruleId = await response.Content.ReadFromJsonAsync<Guid>();
        ruleId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AddAlertRule_ForNonExistentBudget_ShouldReturn404()
    {
        var response = await _client.PostAsJsonAsync($"/api/budgets/{Guid.NewGuid()}/rules", new
        {
            thresholdPercentage = 80m
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddAlertRule_WithZeroThreshold_ShouldReturn400()
    {
        var budgetId = await CreateBudgetAsync();

        var response = await _client.PostAsJsonAsync($"/api/budgets/{budgetId}/rules", new
        {
            thresholdPercentage = 0m
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddAlertRule_WithThresholdAbove100_ShouldReturn400()
    {
        var budgetId = await CreateBudgetAsync();

        var response = await _client.PostAsJsonAsync($"/api/budgets/{budgetId}/rules", new
        {
            thresholdPercentage = 101m
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
