using BudgetAlert.Domain.Events;

namespace BudgetAlert.Domain
{
    public class AggregateRoot
    {
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        #region Private Fields
        private readonly List<IDomainEvent> _domainEvents = [];
        #endregion

        public void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public IReadOnlyList<IDomainEvent> PopDomainEvents()
        {
            var events = _domainEvents.ToList();
            _domainEvents.Clear();
            return events;
        }
    }
}