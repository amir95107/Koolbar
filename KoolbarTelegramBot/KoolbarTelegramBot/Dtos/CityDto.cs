using KoolbarTelegramBot.Dtos;

namespace Koolbar.Dtos
{
    public class CityDto
    {
        public long UniqueKey { get; set; }
        public string Title { get; set; }
        public string? PersianTitle { get; set; }
        public StateDto State { get; set; }
    }
}
