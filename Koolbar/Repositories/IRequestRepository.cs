using Datalayer.Models;

namespace Koolbar.Repositories
{
    public interface IRequestRepository:IBaseRepository<Request, Guid>
    {
        Task<Request> GetRequestByUserIdAsync(Guid userid);
        Task<Request> GetRequestByChatIdAsync(long chatid);
        Task<bool> OpenRequestExistsAsync(Guid userId);
    }
}
