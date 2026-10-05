using System.Net;
using System.Net.Http.Json;
using BudgetAlert.Api.IntegrationTests.Helpers;
using BudgetAlert.Application.Alerts.DTOs;
using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetAlert.Api.IntegrationTests;

public class AlertsEndpointTests(BudgetAlertApiFactory factory) : IClassFixture<BudgetAlertApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetAlerts_WithBudgetIdFilter_ReturnsEmptyForBudgetWithNoAlerts()
    {
        var budgetResponse = await _client.PostAsJsonAsync("/api/budgets", new { name = "Budget", limit = 1000m, currency = "EUR" });
        var budgetId = Guid.Parse(budgetResponse.Headers.Location!.Segments.Last());

        var response = await _client.GetAsync($"/api/alerts?budgetId={budgetId}");
        var body = await response.Content.ReadFromJsonAsync<List<AlertDto>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAlerts_WithBudgetIdFilter_ReturnsOnlyAlertsForThatBudget()
    {
        // Create budget and alert rule via API
        var budgetResponse = await _client.PostAsJsonAsync("/api/budgets", new { name = "Budget", limit = 1000m, currency = "EUR" });
        var budgetId = Guid.Parse(budgetResponse.Headers.Location!.Segments.Last());

        var ruleResponse = await _client.PostAsJsonAsync($"/api/budgets/{budgetId}/rules", new { thresholdPercentage = 80m });
        var ruleId = Guid.Parse(ruleResponse.Headers.Location!.Segments.Last());

        // Seed an alert directly - the Worker would normally do this
        using (var scope = factory.Services.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IAlertRepository>();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            repo.Add(Alert.Create(budgetId, ruleId, 850m, 80m, 1000m));
            await uow.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/alerts?budgetId={budgetId}");
        var body = await response.Content.ReadFromJsonAsync<List<AlertDto>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().ContainSingle(a => a.BudgetId == budgetId && a.ThresholdPercentage == 80m);
    }
}
