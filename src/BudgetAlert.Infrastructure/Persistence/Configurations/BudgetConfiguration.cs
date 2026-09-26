using BudgetAlert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetAlert.Infrastructure.Persistence.Configurations
{
    public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
    {
        public void Configure(EntityTypeBuilder<Budget> builder)
        {
            builder.ToTable("Budgets");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(b => b.Limit)
                .HasPrecision(18, 2);

            builder.Property(b => b.Currency)
                .HasMaxLength(3);

            builder.HasMany(b => b.Transactions)
                .WithOne()
                .HasForeignKey(t => t.BudgetId);
            builder.Navigation(b => b.Transactions)
                .HasField("_transactions")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(b => b.AlertRules)
                .WithOne()
                .HasForeignKey(ar => ar.BudgetId);
            builder.Navigation(b => b.AlertRules)
                .HasField("_alertRules")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}