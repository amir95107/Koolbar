using System.ComponentModel.DataAnnotations.Schema;

namespace Datalayer.Models
{
    [Table(name:nameof(City))]
    public class City : LocationBase
    {
        public long UniqueKey { get; set; }
        public Guid StateId { get; set; }
        public double? Lat { get; set; }
        public double? Long { get; set; }
        public int CityNumber { get; set; }
        public virtual State State { get; set; }        
    }
}
