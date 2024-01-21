using DataLayer.Models.Base;

namespace Datalayer.Models
{
    public class State : GuidAuditableAggregateRoot
    {
        public string Title { get; set; }
        public string EnglishTitle { get; set; }
        public Guid ContryId { get; set; }

        public virtual Country Country { get; set; }
        public virtual ICollection<City> Cities { get; set; } = new HashSet<City>();

        protected override void EnsureReadyState(object @event)
        {
            throw new NotImplementedException();
        }

        protected override void EnsureValidState()
        {
            throw new NotImplementedException();
        }

        protected override void When(object @event)
        {
            throw new NotImplementedException();
        }
    }
}
