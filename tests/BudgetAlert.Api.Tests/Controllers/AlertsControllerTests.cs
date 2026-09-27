using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BudgetAlert.Api.Tests.Helpers;
using BudgetAlert.Domain.Entities;

namespace BudgetAlert.Api.Tests.Controllers;

// No IClassFixture - xUnit creates a fresh instance per test, giving each test an isolated store.
public class AlertsControllerTests : IDisposable
{
    private readonly ApiTestFactory _factory = new();
    private readonly HttpClient _client;

    public AlertsControllerTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetAlerts_ReturnsOkWithEmptyArray()
    {
        var response = await _client.GetAsync("/api/alerts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, body.GetArrayLength());
    }

    [Fact]
    public async Task GetAlerts_WithBudgetId_ReturnsOnlyAlertsForThatBudget()
    {
        var targetBudgetId = Guid.NewGuid();
        _factory.Alerts.Add(Alert.Create(targetBudgetId, Guid.NewGuid(), 850m, 80m, 1000m));
        _factory.Alerts.Add(Alert.Create(targetBudgetId, Guid.NewGuid(), 950m, 90m, 1000m));
        _factory.Alerts.Add(Alert.Create(Guid.NewGuid(), Guid.NewGuid(), 600m, 60m, 1000m));

        var response = await _client.GetAsync($"/api/alerts?budgetId={targetBudgetId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, body.GetArrayLength());
    }
}
