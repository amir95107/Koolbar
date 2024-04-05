using Datalayer.Models;
using DataLayer.Models.Base;
using Koolbar.Dtos;

namespace Koolbar.Repositories
{
    public interface IRequestRepository : IBaseRepository<Request, Guid>
    {
        Task<Request> GetRequestByUserIdAsync(Guid userid);
        Task<Request> GetRequestByChatIdAsync(long chatid);
        Task<Request> GetRequestByKeyAsync(int key);
        Task<List<Request>> GetRequestsByChatIdAsync(long chatid);
        Task<Request> GetCompleteRequestByChatIdAsync(long chatid);
        Task<bool> OpenRequestExistsAsync(Guid userId);
        Task<List<RequestDto>> SuggestAsync(Request request);
        Task<List<RequestDto>> GetAllRequestsAsync(int skip, int take);
        Task<int?> GetLastKeyAsync();

    }
}
