using Datalayer.Enumerations;

namespace Koolbar.Dtos
{
    public class RequestDto
    {
        public Guid UserId { get; set; }
        public long ChatId { get; set; }
        public RequestType RequestType { get; set; }
        public string? Description { get; set; }
        public string? Source { get; set; }
        public string? Destination { get; set; }
        public DateTime? FlightDate { get; set; }
        public DateTime? LimitDate { get; set; }
    }
}
