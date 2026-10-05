using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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
    public async Task AddAlertRule_WithValidData_Returns201WithLocationHeader()
    {
        var budgetId = await CreateBudgetAsync();

        var response = await _client.PostAsJsonAsync($"/api/budgets/{budgetId}/rules", new
        {
            thresholdPercentage = 80m
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains($"/api/budgets/{budgetId}/rules/", response.Headers.Location!.ToString());
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

    [Fact]
    public async Task GetAlertRules_WithExistingBudget_Returns200WithRules()
    {
        var budgetId = await CreateBudgetAsync();
        await _client.PostAsJsonAsync($"/api/budgets/{budgetId}/rules", new { thresholdPercentage = 80m });
        await _client.PostAsJsonAsync($"/api/budgets/{budgetId}/rules", new { thresholdPercentage = 50m });

        var response = await _client.GetAsync($"/api/budgets/{budgetId}/rules");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, body.ValueKind);
        Assert.Equal(2, body.GetArrayLength());
    }

    [Fact]
    public async Task GetAlertRules_ForNonexistentBudget_Returns404()
    {
        var response = await _client.GetAsync($"/api/budgets/{Guid.NewGuid()}/rules");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAlertRuleById_WithExistingRule_Returns200WithRuleData()
    {
        var budgetId = await CreateBudgetAsync();
        var postResponse = await _client.PostAsJsonAsync($"/api/budgets/{budgetId}/rules", new { thresholdPercentage = 80m });
        var ruleId = postResponse.Headers.Location!.Segments.Last();

        var response = await _client.GetAsync($"/api/budgets/{budgetId}/rules/{ruleId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(80m, body.GetProperty("thresholdPercentage").GetDecimal());
        Assert.True(body.GetProperty("isActive").GetBoolean());
    }

    [Fact]
    public async Task GetAlertRuleById_WithNonexistentRule_Returns404()
    {
        var budgetId = await CreateBudgetAsync();

        var response = await _client.GetAsync($"/api/budgets/{budgetId}/rules/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAlertRule_WithValidData_Returns204()
    {
        var budgetId = await CreateBudgetAsync();
        var postResponse = await _client.PostAsJsonAsync($"/api/budgets/{budgetId}/rules", new { thresholdPercentage = 80m });
        var ruleId = postResponse.Headers.Location!.Segments.Last();

        var response = await _client.PatchAsJsonAsync($"/api/budgets/{budgetId}/rules/{ruleId}", new
        {
            thresholdPercentage = 60m,
            isActive = false
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAlertRule_WithNonexistentRule_Returns404()
    {
        var budgetId = await CreateBudgetAsync();

        var response = await _client.PatchAsJsonAsync($"/api/budgets/{budgetId}/rules/{Guid.NewGuid()}", new
        {
            thresholdPercentage = 60m,
            isActive = false
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
