using Datalayer.Models.Base;

namespace Datalayer.Models;
public class Wallet : BaseEntity
{
    public Guid UserId { get; set; }
    public int Amount {  get; set; } 

    public virtual User User { get; set; }
    public virtual IEnumerable<WalletHistory> WalletHistories { get; set; } = new HashSet<WalletHistory>();
}
