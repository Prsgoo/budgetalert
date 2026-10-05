namespace BudgetAlert.Domain.Entities
{
    public class AlertRule
    {
        public Guid Id { get; private set; }
        public Guid BudgetId { get; private set; }
        public decimal ThresholdPercentage { get; private set; }  // e.g. 80.0 = 80%
        public bool IsActive { get; private set; }

        public static AlertRule Create(Guid budgetId, decimal thresholdPercentage)
        {
            if (thresholdPercentage < 1 || thresholdPercentage > 100)
                throw new ArgumentOutOfRangeException(nameof(thresholdPercentage), "Threshold percentage must be between 1 and 100.");

            return new AlertRule
            {
                Id = Guid.NewGuid(),
                BudgetId = budgetId,
                ThresholdPercentage = thresholdPercentage,
                IsActive = true
            };
        }

        public void Update(decimal thresholdPercentage, bool isActive)
        {
            if (thresholdPercentage < 1 || thresholdPercentage > 100)
                throw new ArgumentOutOfRangeException(nameof(thresholdPercentage), "Threshold percentage must be between 1 and 100.");
            ThresholdPercentage = thresholdPercentage;
            IsActive = isActive;
        }
    }
}