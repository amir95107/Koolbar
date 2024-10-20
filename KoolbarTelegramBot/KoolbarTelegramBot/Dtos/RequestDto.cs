﻿using Datalayer.Enumerations;

namespace Koolbar.Dtos
{
    public class RequestDto
    {
        public Guid? UserId { get; set; }
        public string? Username { get; set; }
        public long ChatId { get; set; }
        public bool IsVerified { get; set; }
        public bool IsBanned { get; set; }
        public int Key { get; set; }
        public RequestType? RequestType { get; set; }
        public RequestStatus RequestStatus { get; set; }
        public string? Description { get; set; }      
        public DateTime? FlightDate { get; set; }
        public DateTime? LimitDate { get; set; }
        public string? FlightMonth { get; set; }
        public int? FlightDay { get; set; }
        public int? MessageId {  get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CreatedAt { get; set; }
        public CityDto Source { get; set; }
        public CityDto Destination { get; set; }
    }
}
