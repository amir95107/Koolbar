using Datalayer.Data;
using Datalayer.Enumerations;
using Datalayer.Models;
using DataLayer.Models.Base;
using Koolbar.Dtos;
using Koolbar.Repositories;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;

namespace Koolbar.Services
{
    public class RequestRepository : BaseRepository<Request, Guid>, IRequestRepository
    {
        private readonly IQueryable<Request> NotRemoved;
        public RequestRepository(IHttpContextAccessor accessor, ApplicationDBContext context) : base(accessor, context)
        {
            NotRemoved = Entities.Where(x => x.RemovedAt == null);
        }

        public async Task<Request> GetRequestByChatIdAsync(long chatid)
        {
            try
            {
                return await NotRemoved
                .Include(x => x.User)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(x => x.User.ChatId == chatid && !x.IsCompleted);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<List<Request>> GetRequestsByChatIdAsync(long chatid)
        {
            try
            {
                return await NotRemoved
                .Include(x => x.User)
                .OrderByDescending(x => x.CreatedAt)
                .Where(x => x.User.ChatId == chatid && !x.IsCompleted)
                .ToListAsync();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<Request> GetRequestByUserIdAsync(Guid userid)
            => await NotRemoved
            .FirstOrDefaultAsync(x => x.UserId == userid);

        public async Task<bool> OpenRequestExistsAsync(Guid userId)
            => await NotRemoved
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.RequestStatus == RequestStatus.New);

        public async Task<List<RequestDto>> GetAllRequestsAsync(int skip, int take)
            => await Entities
            .Include(x => x.User)
            .Skip(skip)
            .Take(take)
            .Select(x => new RequestDto
            {
                Id = x.Id,
                ChatId = x.User.ChatId,
                UserId = x.UserId,
                RequestStatus = x.RequestStatus,
                Description = x.Description,
                Destination = x.Destination,
                FlightDate = x.FlightDate,
                LimitDate = x.LimitDate,
                RequestType = x.RequestType,
                Source = x.Source,
                Username = x.User.UserName,
                CreatedAt = x.CreatedAt
            })
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        public async Task<List<Request>> SuggestAsync(Request request)
            => await Entities
            .Include(x => x.User)
            .Where(x => x.IsCompleted && x.RequestType != request.RequestType && x.UserId != request.UserId &&
            (x.Source == request.Destination || x.Destination == request.Source) &&
            (request.RequestType == RequestType.FreightOwner ? x.RequestType == RequestType.Passenger && x.FlightDate < DateTime.Now : true))
            .ToListAsync();

        public async Task<Request> GetCompleteRequestByChatIdAsync(long chatid)
            => await NotRemoved
                .Include(x => x.User)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(x => x.User.ChatId == chatid && x.IsCompleted);
    }
}
