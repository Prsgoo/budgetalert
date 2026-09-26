namespace BudgetAlert.Domain.Entities
{
    public class Budget : AggregateRoot
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public decimal Limit { get; private set; }
        public string Currency { get; private set; } = string.Empty; // ISO 4217, e.g. "EUR"
        public DateTime CreatedAt { get; private set; }
        public IReadOnlyCollection<Transaction> Transactions => _transactions.ToList();
        public IReadOnlyCollection<AlertRule> AlertRules => _alertRules.ToList();

        #region Private fields
        private readonly List<Transaction> _transactions = [];
        private readonly List<AlertRule> _alertRules = [];
        #endregion

        public static Budget Create(string name, decimal limit, string currency)
        {
            return new Budget
            {
                Id = Guid.NewGuid(),
                Name = name,
                Limit = limit,
                Currency = currency,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void RegisterTransaction(decimal amount, string description, DateTime occurredAt)
        {
            var transaction = Transaction.Create(Id, amount, description, occurredAt);
            _transactions.Add(transaction);
            AddDomainEvent(new Events.TransactionRegistered(
                EventId: Guid.NewGuid(),
                OccurredAt: DateTime.UtcNow,
                BudgetId: Id,
                TransactionId: transaction.Id,
                Amount: amount,
                CurrentSpend: CurrentSpend,
                BudgetLimit: Limit
            ));
        }

        public Guid AddAlertRule(decimal thresholdPercentage)
        {
            var alertRule = AlertRule.Create(Id, thresholdPercentage);
            _alertRules.Add(alertRule);
            return alertRule.Id;
        }

        public decimal CurrentSpend => Transactions.Sum(t => t.Amount);

        public decimal SpendPercentage => Limit == 0 ? 0 : (CurrentSpend / Limit) * 100;
    }
}