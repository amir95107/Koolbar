using DataLayer.Models.Base;

namespace Datalayer.Models
{
    public class Country : LocationBase
    {

        public int CountryNumber { get; set; }


        public virtual ICollection<State> States { get; set; }
    }
}
