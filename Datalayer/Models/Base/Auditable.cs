using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datalayer.Models.Base
{
    public abstract class Auditable : Creatable
    {
        public DateTime ModifiedAt { get; set; }
        public Guid? ModifiedBy { get; set; }

        public Auditable()
        {
            ModifiedAt = DateTime.Now;
            ModifiedBy = Guid.Parse("");
        }
    }
}
