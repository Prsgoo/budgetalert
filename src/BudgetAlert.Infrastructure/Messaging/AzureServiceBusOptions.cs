namespace BudgetAlert.Infrastructure.Messaging
{
    public class AzureServiceBusOptions
    {
        public string? ConnectionString { get; init; }         // emulator only
        public string? FullyQualifiedNamespace { get; init; }  // real Azure + DefaultAzureCredential
        public string QueueName { get; init; } = "budget-events";
    }
}
