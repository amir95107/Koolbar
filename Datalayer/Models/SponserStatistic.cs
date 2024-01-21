using DataLayer.Models.Base;

namespace Datalayer.Models
{
    public class SponserStatistics : GuidAuditableEntity
    {
        public Guid SponserId { get; set; }


        public virtual Sponser Sponser { get; set; }

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
