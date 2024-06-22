using Datalayer.Enumerations;
using Datalayer.Models;
using DataLayer.Models.Base;
using Koolbar.Dtos;
using Koolbar.Services;

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
        Task<List<RequestDto>> GetAllRequestsAsync(RequestType? type, string? source, string? destination, int take = 10, int p = 1);
        Task<int?> GetLastKeyAsync();
        Task<List<PopularCity>> MostPopularCitiesAsync(LocationType type);

    }
}
