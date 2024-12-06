using DataLayer.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Datalayer.Models
{
    [Table(name: nameof(State))]

    public class State : LocationBase
    {
        public long UniqueKey { get; set; }
        public Guid ContryId { get; set; }
        public int StateNumber { get; set; }

        public virtual Country Country { get; set; }
        public virtual ICollection<City> Cities { get; set; } = new HashSet<City>();

        
    }
}
