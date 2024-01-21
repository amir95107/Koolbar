using Datalayer.Enumerations;
using DataLayer.Models.Base;

namespace Datalayer.Models
{
    public class Sponser : GuidAuditableAggregateRoot
    {
        public string Title { get; set; }
        public long ChatId { get; set; }
        public string? Username { get; set; }
        public ChannelType MyProperty { get; set; }
        public bool IsActive { get; set; }
        public virtual SponserStatistics Statistics { get; set; }
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
