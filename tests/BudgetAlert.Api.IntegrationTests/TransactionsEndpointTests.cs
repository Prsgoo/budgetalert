using System.Net;
using System.Net.Http.Json;
using BudgetAlert.Api.IntegrationTests.Helpers;
using BudgetAlert.Application.Budgets.DTOs;
using FluentAssertions;

namespace BudgetAlert.Api.IntegrationTests;

public class TransactionsEndpointTests(BudgetAlertApiFactory factory) : IClassFixture<BudgetAlertApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<Guid> CreateBudgetAsync(decimal limit = 1000m)
    {
        var response = await _client.PostAsJsonAsync("/api/budgets", new { name = "Budget", limit, currency = "EUR" });
        response.EnsureSuccessStatusCode();
        return Guid.Parse(response.Headers.Location!.Segments.Last());
    }

    [Fact]
    public async Task RegisterTransaction_ShouldReturn201WithTransactionId()
    {
        var budgetId = await CreateBudgetAsync();

        var response = await _client.PostAsJsonAsync($"/api/budgets/{budgetId}/transactions", new
        {
            amount = 200m,
            description = "Groceries",
            occurredAt = DateTime.UtcNow.AddHours(-1)
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task RegisterTransaction_ForNonExistentBudget_ShouldReturn404()
    {
        var response = await _client.PostAsJsonAsync($"/api/budgets/{Guid.NewGuid()}/transactions", new
        {
            amount = 100m,
            description = "Test",
            occurredAt = DateTime.UtcNow.AddHours(-1)
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RegisterTransaction_WithZeroAmount_ShouldReturn400()
    {
        var budgetId = await CreateBudgetAsync();

        var response = await _client.PostAsJsonAsync($"/api/budgets/{budgetId}/transactions", new
        {
            amount = 0m,
            description = "Test",
            occurredAt = DateTime.UtcNow.AddHours(-1)
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterTransaction_WithEmptyDescription_ShouldReturn400()
    {
        var budgetId = await CreateBudgetAsync();

        var response = await _client.PostAsJsonAsync($"/api/budgets/{budgetId}/transactions", new
        {
            amount = 100m,
            description = "",
            occurredAt = DateTime.UtcNow.AddHours(-1)
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterTransaction_WithFutureDate_ShouldReturn400()
    {
        var budgetId = await CreateBudgetAsync();

        var response = await _client.PostAsJsonAsync($"/api/budgets/{budgetId}/transactions", new
        {
            amount = 100m,
            description = "Test",
            occurredAt = DateTime.UtcNow.AddDays(1)
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterTransaction_UpdatesBudgetCurrentSpend()
    {
        var budgetId = await CreateBudgetAsync(limit: 1000m);

        await _client.PostAsJsonAsync($"/api/budgets/{budgetId}/transactions", new
        {
            amount = 300m,
            description = "Groceries",
            occurredAt = DateTime.UtcNow.AddHours(-1)
        });

        var response = await _client.GetAsync($"/api/budgets/{budgetId}");
        var body = await response.Content.ReadFromJsonAsync<BudgetDto>();

        body!.CurrentSpend.Should().Be(300m);
        body.SpendPercentage.Should().Be(30m);
    }
}
