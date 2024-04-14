using DataLayer.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace Datalayer.Models
{
    public class Country : LocationBase
    {

        public int CountryNumber { get; set; }
        [MaxLength(25)]
        public string? Emoji { get; set; }

        public virtual ICollection<State> States { get; set; }
    }
}
