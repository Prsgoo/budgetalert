using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BudgetAlert.Api.Tests.Helpers;

namespace BudgetAlert.Api.Tests.Controllers;

public class TransactionsControllerTests(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
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
    public async Task RegisterTransaction_WithValidBody_Returns201WithTransactionId()
    {
        var budgetId = await CreateBudgetAsync();

        var response = await _client.PostAsJsonAsync($"/api/budgets/{budgetId}/transactions", new
        {
            amount = 200m,
            description = "Groceries",
            occurredAt = DateTime.UtcNow.AddHours(-1)
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.TryGetProperty("transactionId", out var transactionId));
        Assert.Equal(JsonValueKind.String, transactionId.ValueKind);
    }

    [Fact]
    public async Task RegisterTransaction_ForNonexistentBudget_Returns404()
    {
        var response = await _client.PostAsJsonAsync($"/api/budgets/{Guid.NewGuid()}/transactions", new
        {
            amount = 100m,
            description = "Test",
            occurredAt = DateTime.UtcNow.AddHours(-1)
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RegisterTransaction_WithZeroAmount_Returns400()
    {
        var budgetId = await CreateBudgetAsync();

        var response = await _client.PostAsJsonAsync($"/api/budgets/{budgetId}/transactions", new
        {
            amount = 0m,
            description = "Test",
            occurredAt = DateTime.UtcNow.AddHours(-1)
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterTransaction_WithEmptyDescription_Returns400()
    {
        var budgetId = await CreateBudgetAsync();

        var response = await _client.PostAsJsonAsync($"/api/budgets/{budgetId}/transactions", new
        {
            amount = 100m,
            description = "",
            occurredAt = DateTime.UtcNow.AddHours(-1)
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTransactions_WithExistingBudget_Returns200WithTransactions()
    {
        var budgetId = await CreateBudgetAsync();
        await _client.PostAsJsonAsync($"/api/budgets/{budgetId}/transactions", new { amount = 100m, description = "A", occurredAt = DateTime.UtcNow.AddHours(-1) });
        await _client.PostAsJsonAsync($"/api/budgets/{budgetId}/transactions", new { amount = 200m, description = "B", occurredAt = DateTime.UtcNow.AddHours(-2) });

        var response = await _client.GetAsync($"/api/budgets/{budgetId}/transactions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, body.ValueKind);
        Assert.Equal(2, body.GetArrayLength());
    }

    [Fact]
    public async Task GetTransactions_ForNonexistentBudget_Returns404()
    {
        var response = await _client.GetAsync($"/api/budgets/{Guid.NewGuid()}/transactions");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
