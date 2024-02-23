using DataLayer.Models.Base;

namespace Datalayer.Models
{
    public class City : LocationBase
    {
        public Guid StateId { get; set; }
        public double? Lat { get; set; }
        public double? Long { get; set; }
        public int CityNumber { get; set; }
        public virtual State State { get; set; }

        
    }
}
