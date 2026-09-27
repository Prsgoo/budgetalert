using BudgetAlert.Application.Alerts.Queries;
using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Repositories;
using Moq;

namespace BudgetAlert.Application.Tests.Alerts.Queries;

public class GetAlertsQueryHandlerTests
{
    private readonly Mock<IAlertRepository> _repo = new();

    private static Alert CreateAlert(Guid budgetId) =>
        Alert.Create(budgetId, Guid.NewGuid(), 800m, 80m, 1000m);

    [Fact]
    public async Task Handle_ReturnsAllAlertsWhenBudgetIdIsNull()
    {
        var alerts = new List<Alert> { CreateAlert(Guid.NewGuid()), CreateAlert(Guid.NewGuid()) };
        _repo.Setup(r => r.GetAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(alerts);
        var handler = new GetAlertsQueryHandler(_repo.Object);

        var result = await handler.Handle(new GetAlertsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_PassesBudgetIdToRepository()
    {
        var budgetId = Guid.NewGuid();
        _repo.Setup(r => r.GetAsync(budgetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([CreateAlert(budgetId)]);
        var handler = new GetAlertsQueryHandler(_repo.Object);

        await handler.Handle(new GetAlertsQuery(budgetId), CancellationToken.None);

        _repo.Verify(r => r.GetAsync(budgetId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OrdersByTriggeredAtDescending()
    {
        var budgetId = Guid.NewGuid();
        var alert1 = CreateAlert(budgetId);
        System.Threading.Thread.Sleep(50);
        var alert2 = CreateAlert(budgetId);

        // Pass in ascending order; handler should sort descending
        _repo.Setup(r => r.GetAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([alert1, alert2]);
        var handler = new GetAlertsQueryHandler(_repo.Object);

        var result = await handler.Handle(new GetAlertsQuery(), CancellationToken.None);

        Assert.True(result[0].TriggeredAt >= result[1].TriggeredAt);
    }

    [Fact]
    public async Task Handle_MapsAlertToAlertDtoCorrectly()
    {
        var budgetId = Guid.NewGuid();
        var alert = Alert.Create(budgetId, Guid.NewGuid(), 800m, 80m, 1000m);
        _repo.Setup(r => r.GetAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([alert]);
        var handler = new GetAlertsQueryHandler(_repo.Object);

        var result = await handler.Handle(new GetAlertsQuery(), CancellationToken.None);

        var dto = result.Single();
        Assert.Equal(alert.Id, dto.Id);
        Assert.Equal(budgetId, dto.BudgetId);
        Assert.Equal(800m, dto.SpendAtTrigger);
        Assert.Equal(80m, dto.ThresholdPercentage);
        Assert.Equal(1000m, dto.BudgetLimit);
    }
}
