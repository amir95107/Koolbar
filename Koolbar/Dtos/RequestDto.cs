using Datalayer.Enumerations;

namespace Koolbar.Dtos
{
    public class RequestDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public long ChatId { get; set; }
        public long MessageId { get; set; }
        public RequestType? RequestType { get; set; }
        public RequestStatus RequestStatus { get; set; }
        public string? Source { get; set; }
        public string? Username { get; set; }
        public string? Destination { get; set; }
        public DateTime? FlightDate { get; set; }
        public DateTime? LimitDate { get; set; }
        public string? Description { get; set; }
        public string? ErrorMessage {  get; set; } 
        public DateTime? CreatedAt { get; set; }

        //public List<RequestDto> Requests { get; set; }
    }
}
