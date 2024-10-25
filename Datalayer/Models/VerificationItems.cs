using DataLayer.Models.Base;

namespace Datalayer.Models
{
    public class VerificationItems : GuidAuditableEntity
    {
        public string Title { get; set; }
        

        protected override void EnsureReadyState(object @event)
        {
        }

        protected override void EnsureValidState()
        {
        }

        protected override void When(object @event)
        {
        }
    }
}
