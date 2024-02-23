using DataLayer.Models.Events;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.Models.Base
{
    public interface IEntity
    {
    }
    public interface IEntity<TKey> : IEntity
    {
        TKey Id { get; set; }
    }
    public interface ICreatableEntity
    {
        DateTime CreatedAt { get; set; }

        Guid CreatedBy { get; set; }
    }
    public interface IModifiableEntity
    {
        DateTime ModifiedAt { get; set; }

        Guid ModifiedBy { get; set; }
    }
    public interface IRemovableEntity
    {
        DateTime? RemovedAt { get; set; }

        Guid? RemovedBy { get; set; }
    }
    public interface IAuditableEntity : ICreatableEntity, IModifiableEntity, IRemovableEntity
    {
    }
    public abstract class GuidAuditableEntity : AuditableEntity<Guid>
    {
    }
    public abstract class AuditableEntity<TKey> : EntityBase<Guid>, IAuditableEntity, ICreatableEntity, IModifiableEntity, IRemovableEntity
    {
        public DateTime CreatedAt { get; set; }

        public Guid CreatedBy { get; set; }

        public DateTime ModifiedAt { get; set; }

        public Guid ModifiedBy { get; set; }

        public DateTime? RemovedAt { get; set; }

        public Guid? RemovedBy { get; set; }
    }
    public abstract class GuidAuditableAggregateRoot : AuditableAggregateRoot<Guid>
    {
    }
    public abstract class AuditableAggregateRoot<TKey> : AggregateRoot<TKey>, IAuditableEntity, ICreatableEntity, IModifiableEntity, IRemovableEntity
    {
        public DateTime CreatedAt { get; set; }

        public Guid CreatedBy { get; set; }

        public DateTime ModifiedAt { get; set; }

        public Guid ModifiedBy { get; set; }

        public DateTime? RemovedAt { get; set; }

        public Guid? RemovedBy { get; set; }
    }
    public abstract class AggregateRoot<TKey> : IEntity<TKey>, IEntity
    {
        private List<DomainEvent> _events;

        public TKey Id { get; set; }

        protected AggregateRoot()
        {
            _events = new List<DomainEvent>();
        }

        protected void AddEvent(DomainEvent @event)
        {
            _events.Add(@event);
        }

        public IEnumerable<DomainEvent> GetChanges()
        {
            return _events;
        }

        public void CLearChanges()
        {
            _events = new List<DomainEvent>();
        }

        public void Apply(DomainEvent @event)
        {
            EnsureReadyState(@event);
            When(@event);
            EnsureValidState();
            _events.Add(@event);
        }

        protected abstract void EnsureReadyState(object @event);

        protected abstract void When(object @event);

        protected abstract void EnsureValidState();
    }
    public abstract class GuidCreatableEntity : CreatableEntity<Guid>
    {
    }
    public abstract class CreatableEntity<TKey> : EntityBase<TKey>, ICreatableEntity
    {
        public DateTime CreatedAt { get; set; }

        public Guid CreatedBy { get; set; }
    }
    public abstract class EntityBase<Guid> : IEntity<Guid>, IEntity
    {
        public Guid Id { get; set; }

        public void Apply(object @event)
        {
            EnsureReadyState(@event);
            When(@event);
            EnsureValidState();
        }

        protected abstract void EnsureReadyState(object @event);

        protected abstract void When(object @event);

        protected abstract void EnsureValidState();
    }
}
