namespace BudgetAlert.Domain.Repositories
{
    // BudgetAlert.Domain/Repositories/IUnitOfWork.cs
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        void Register<T>(T entity) where T : class;
    }
}