using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoolbarTelegramBot.Dtos
{
    public class StateDto
    {
        public string Title { get; set; }
        public string? PersianTitle { get; set; }
        public CountryDto Country { get; set; }
    }
}
