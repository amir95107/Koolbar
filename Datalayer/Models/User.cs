using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Datalayer.Models
{
    public partial class User : IdentityUser<Guid>
    {
        public long ChatId { get; set; }
        public bool MuteNotification { get; set; }
        public Guid? UniqueCode { get; set; } = Guid.NewGuid();
        [Required]
        public bool IsBanned { get; set; }
        public DateTime? BannedAt { get; set; }
        [Required]
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime? VerifiedUntil { get; set; }

        public virtual ICollection<Request> Requests { get; set; } = new HashSet<Request>();
        public virtual Wallet Wallet { get; set; }
    }
}
