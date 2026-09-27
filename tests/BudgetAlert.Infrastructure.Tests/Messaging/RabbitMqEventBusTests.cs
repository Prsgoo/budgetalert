using System.Text.Json;
using BudgetAlert.Domain.Events;

namespace BudgetAlert.Infrastructure.Tests.Messaging;

public class RabbitMqEventBusTests
{
    [Fact]
    public void RoutingKey_ForTransactionRegistered_IsLowercaseTypeName()
    {
        var routingKey = typeof(TransactionRegistered).Name.ToLowerInvariant();

        Assert.Equal("transactionregistered", routingKey);
    }

    [Fact]
    public void SerializedBody_IsValidJson()
    {
        var evt = new TransactionRegistered(
            EventId: Guid.NewGuid(),
            OccurredAt: DateTime.UtcNow,
            BudgetId: Guid.NewGuid(),
            TransactionId: Guid.NewGuid(),
            Amount: 100m,
            CurrentSpend: 100m,
            BudgetLimit: 1000m
        );

        var body = JsonSerializer.SerializeToUtf8Bytes(evt);

        // Parse throws on invalid JSON - if it returns, the body is valid
        var json = JsonDocument.Parse(body);
        Assert.Equal(JsonValueKind.Object, json.RootElement.ValueKind);
    }

    [Fact]
    public void SerializedBody_ContainsBudgetId()
    {
        var budgetId = Guid.NewGuid();
        var evt = new TransactionRegistered(
            EventId: Guid.NewGuid(),
            OccurredAt: DateTime.UtcNow,
            BudgetId: budgetId,
            TransactionId: Guid.NewGuid(),
            Amount: 100m,
            CurrentSpend: 100m,
            BudgetLimit: 1000m
        );

        var body = JsonSerializer.SerializeToUtf8Bytes(evt);
        var json = JsonDocument.Parse(body);

        Assert.Equal(budgetId.ToString(), json.RootElement.GetProperty("BudgetId").GetString());
    }

    [Fact]
    public void SerializedBody_ContainsAmount()
    {
        var evt = new TransactionRegistered(
            EventId: Guid.NewGuid(),
            OccurredAt: DateTime.UtcNow,
            BudgetId: Guid.NewGuid(),
            TransactionId: Guid.NewGuid(),
            Amount: 250m,
            CurrentSpend: 250m,
            BudgetLimit: 1000m
        );

        var body = JsonSerializer.SerializeToUtf8Bytes(evt);
        var json = JsonDocument.Parse(body);

        Assert.Equal(250m, json.RootElement.GetProperty("Amount").GetDecimal());
    }
}
