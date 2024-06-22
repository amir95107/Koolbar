using Datalayer.Enumerations;
using DataLayer.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Datalayer.Models
{
    public class Request : GuidAuditableAggregateRoot
    {
        public Guid UserId { get; set; }
        public int Key {  get; set; }
        public RequestType? RequestType { get; set; }
        public string? Description { get; set; }

        public string? Source { get; set; }
        [NotMapped]
        public Guid? SourceCityId { get; set; }
        public string? Destination { get; set; }
        [NotMapped]
        public Guid? DestinationCityId { get; set; }
        public DateTime? FlightDate { get; set; }
        public DateTime? LimitDate { get; set; }

        public RequestStatus RequestStatus { get; set; } = RequestStatus.New;
        public bool IsCompleted { get; set; }
        public long MessageId { get; set; }
        public Platform Platform { get; set; } = Platform.Telegram;




        public virtual User User { get; set; }

        public Request()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.Now;
            ModifiedAt = DateTime.Now;
        }

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
