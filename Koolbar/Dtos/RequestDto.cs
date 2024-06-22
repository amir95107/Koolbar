using Datalayer.Enumerations;

namespace Koolbar.Dtos
{
    public class RequestDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int Key { get; set; }
        public long ChatId { get; set; }
        public long MessageId { get; set; }
        public RequestType? RequestType { get; set; }
        public RequestStatus RequestStatus { get; set; }
        public CityDto Source { get; set; }
        public string? Username { get; set; }
        public CityDto Destination { get; set; }
        public DateTime? FlightDate { get; set; }
        public DateTime? LimitDate { get; set; }
        public string? Description { get; set; }
        public string? ErrorMessage {  get; set; } 
        //public DateTime? CreatedAt { get; set; }

        //public List<RequestDto> Requests { get; set; }
    }

    public class RequestsDto
    {
        public string? Username { get; set; }
        public long ChatId { get; set; }
        public RequestType RequestType { get; set; }
        public string Source { get; set; }
        public string? SourceCityId { get; set; }
        public string Destination { get; set; }
        public string? DestinationCityId { get; set; }
        public DateTime FlightDate { get; set; }
        public string Description { get; set; }
    }

    public class RequestContract
    {
        //public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public int Key { get; set; }
        public long ChatId { get; set; }
        public long MessageId { get; set; }
        public RequestType? RequestType { get; set; }
        public RequestStatus RequestStatus { get; set; }
        public CityDto Source { get; set; }
        public CityDto Destination { get; set; }
        public string? Username { get; set; }
        
        public DateTime? FlightDate { get; set; }
        public DateTime? LimitDate { get; set; }
        public string? Description { get; set; }
        //public DateTime? CreatedAt { get; set; }

        //public List<RequestDto> Requests { get; set; }
    }

    public class CityDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? PersianTitle { get; set; }
        public StateDto State { get; set; }
    }

    public class StateDto
    {
        public string Title { get; set; }
        public string? PersianTitle { get; set; }
        public CountryDto Country { get; set; }
    }

    public class CountryDto
    {
        public string Title { get; set; }
        public string? PersianTitle { get; set; }
        public string? Emoji { get; set; }
    }
}
