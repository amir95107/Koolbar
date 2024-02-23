using DataLayer.Models.Base;

namespace Datalayer.Models
{
    public class State : LocationBase
    {
        public Guid ContryId { get; set; }
        public int StateNumber { get; set; }

        public virtual Country Country { get; set; }
        public virtual ICollection<City> Cities { get; set; } = new HashSet<City>();

        
    }
}
