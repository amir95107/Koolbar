using DataLayer.Models.Base;

namespace Datalayer.Models
{
    public class City : GuidAuditableEntity
    {
        public string Title { get; set; }
        public string EnglishTitle { get; set; }
        public Guid StateId { get; set; }
        public virtual State State { get; set; }

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
