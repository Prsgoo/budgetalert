using BudgetAlert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetAlert.Infrastructure.Persistence.Configurations
{
    public class AlertConfiguration : IEntityTypeConfiguration<Alert>
    {
        public void Configure(EntityTypeBuilder<Alert> builder)
        {
            builder.ToTable("Alerts");

            builder.HasKey(a => a.Id);

            builder.HasOne<Budget>()
                .WithMany()
                .HasForeignKey(a => a.BudgetId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne<AlertRule>()
                .WithMany()
                .HasForeignKey(a => a.AlertRuleId);

            builder.Property(a => a.SpendAtTrigger)
                .HasPrecision(18, 2);

            builder.Property(a => a.TriggeredAt)
                .IsRequired();

            builder.Property(a => a.ThresholdPercentage)
                .HasPrecision(5, 2);

            builder.Property(a => a.BudgetLimit)
                .HasPrecision(18, 2);
        }
    }
}