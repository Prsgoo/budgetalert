namespace BudgetAlert.Infrastructure.Messaging
{
    public class AzureServiceBusOptions
    {
        // Used by emulator; replaced by FullyQualifiedNamespace + DefaultAzureCredential in PRS-105
        public string ConnectionString { get; init; } = string.Empty;
        public string QueueName { get; init; } = "budget-events";
    }
}
