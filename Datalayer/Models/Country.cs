using DataLayer.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Datalayer.Models
{
    [Table(name: nameof(Country))]
    public class Country : LocationBase
    {
        public long UniqueKey { get; set; }
        public int CountryNumber { get; set; }
        [MaxLength(25)]
        public string? Emoji { get; set; }

        public virtual ICollection<State> States { get; set; }
    }
}
