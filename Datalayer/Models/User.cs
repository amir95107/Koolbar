using Microsoft.AspNetCore.Identity;

namespace Datalayer.Models
{
    public partial class User : IdentityUser<Guid>
    {
        public long ChatId { get; set; }

        public virtual ICollection<Request> Requests { get; set; } = new HashSet<Request>();
    }
}
