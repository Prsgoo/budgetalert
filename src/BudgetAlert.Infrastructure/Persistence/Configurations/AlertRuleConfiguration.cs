using BudgetAlert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetAlert.Infrastructure.Persistence.Configurations
{
    public class AlertRuleConfiguration : IEntityTypeConfiguration<AlertRule>
    {
        public void Configure(EntityTypeBuilder<AlertRule> builder)
        {
            builder.ToTable("AlertRules");

            builder.HasKey(ar => ar.Id);

            builder.Property(ar => ar.ThresholdPercentage)
                .HasPrecision(5, 2);

            builder.Property(ar => ar.IsActive)
                .IsRequired();
        }
    }
}