using KoolbarTelegramBot.Dtos;

namespace Koolbar.Dtos
{
    public class CityDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? PersianTitle { get; set; }
        public Guid StateId { get; set; }
        public double? Lat { get; set; }
        public double? Long { get; set; }
        public string StateTitle { get; set; }
        public string CountryTitle { get; set; }
        public StateDto State { get; set; }
    }
}
