using Datalayer.Models.Base;

namespace Datalayer.Models;
public class WalletHistory:BaseEntity
{
    public Guid WalletId { get; set; }
    public int Amount { get; set; }
    public TransactionType TransactionType {  get; set; } 

    public virtual Wallet Wallet { get; set; }
}

public enum TransactionType
{
    Add=1,
    Expend=2
}
