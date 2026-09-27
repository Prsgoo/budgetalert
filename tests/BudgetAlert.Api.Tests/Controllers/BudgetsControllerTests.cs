using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BudgetAlert.Api.Tests.Helpers;

namespace BudgetAlert.Api.Tests.Controllers;

public class BudgetsControllerTests(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateBudget_WithValidBody_Returns201WithLocationHeader()
    {
        var response = await _client.PostAsJsonAsync("/api/budgets", new
        {
            name = "Test Budget",
            limit = 1000m,
            currency = "EUR"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task CreateBudget_WithEmptyName_Returns400WithFieldErrors()
    {
        var response = await _client.PostAsJsonAsync("/api/budgets", new
        {
            name = "",
            limit = 1000m,
            currency = "EUR"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task CreateBudget_WithZeroLimit_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/budgets", new
        {
            name = "Budget",
            limit = 0m,
            currency = "EUR"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateBudget_WithInvalidCurrencyLength_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/budgets", new
        {
            name = "Budget",
            limit = 500m,
            currency = "US"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetBudget_WithExistingId_Returns200WithBudgetData()
    {
        var postResponse = await _client.PostAsJsonAsync("/api/budgets", new
        {
            name = "Savings",
            limit = 500m,
            currency = "USD"
        });
        var id = postResponse.Headers.Location!.Segments.Last();

        var response = await _client.GetAsync($"/api/budgets/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Savings", body.GetProperty("name").GetString());
        Assert.Equal(500m, body.GetProperty("limit").GetDecimal());
        Assert.Equal("USD", body.GetProperty("currency").GetString());
    }

    [Fact]
    public async Task GetBudget_WithNonexistentId_Returns404()
    {
        var response = await _client.GetAsync($"/api/budgets/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
