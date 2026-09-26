using BudgetAlert.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BudgetAlert.Infrastructure.Persistence.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Amount)
                .HasPrecision(18, 2);

            builder.Property(t => t.Description)
                .HasMaxLength(200);

            builder.Property(t => t.OccurredAt)
                .IsRequired();
        }
    }
}