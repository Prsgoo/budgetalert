using System.Net;
using System.Net.Http.Json;
using BudgetAlert.Api.IntegrationTests.Helpers;
using BudgetAlert.Application.Budgets.DTOs;
using BudgetAlert.Domain.Events;
using FluentAssertions;

namespace BudgetAlert.Api.IntegrationTests;

public class BudgetsEndpointTests(BudgetAlertApiFactory factory) : IClassFixture<BudgetAlertApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<Guid> CreateBudgetAsync(string name = "Test Budget", decimal limit = 1000m, string currency = "EUR")
    {
        var response = await _client.PostAsJsonAsync("/api/budgets", new { name, limit, currency });
        response.EnsureSuccessStatusCode();
        return Guid.Parse(response.Headers.Location!.Segments.Last());
    }

    [Fact]
    public async Task PostBudget_ShouldReturn201WithLocation()
    {
        var response = await _client.PostAsJsonAsync("/api/budgets", new
        {
            name = "Monthly",
            limit = 500m,
            currency = "USD"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task PostBudget_WithInvalidBody_ShouldReturn400()
    {
        var response = await _client.PostAsJsonAsync("/api/budgets", new
        {
            name = "",
            limit = 0m,
            currency = "EUR"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetBudget_AfterCreate_ShouldReturnBudgetDto()
    {
        var id = await CreateBudgetAsync("Savings", 2000m, "GBP");

        var response = await _client.GetAsync($"/api/budgets/{id}");
        var body = await response.Content.ReadFromJsonAsync<BudgetDto>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Name.Should().Be("Savings");
        body.Limit.Should().Be(2000m);
        body.Currency.Should().Be("GBP");
    }

    [Fact]
    public async Task GetBudget_ForNonExistentId_ShouldReturn404()
    {
        var response = await _client.GetAsync($"/api/budgets/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostTransaction_ShouldReturn201AndPublishEvent()
    {
        var budgetId = await CreateBudgetAsync();

        var response = await _client.PostAsJsonAsync($"/api/budgets/{budgetId}/transactions", new
        {
            amount = 150m,
            description = "Groceries",
            occurredAt = DateTime.UtcNow.AddHours(-1)
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        factory.EventBus.Published
            .OfType<TransactionRegistered>()
            .Should().ContainSingle(e => e.BudgetId == budgetId);
    }
}
