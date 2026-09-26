using BudgetAlert.Domain.Entities;
using BudgetAlert.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BudgetAlert.Infrastructure.Persistence
{
    public class BudgetAlertDbContext : DbContext, IUnitOfWork
    {
        public DbSet<Budget> Budgets => Set<Budget>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<AlertRule> AlertRules => Set<AlertRule>();
        public DbSet<Alert> Alerts => Set<Alert>();

        public BudgetAlertDbContext(DbContextOptions<BudgetAlertDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BudgetAlertDbContext).Assembly);
        }
    }
}