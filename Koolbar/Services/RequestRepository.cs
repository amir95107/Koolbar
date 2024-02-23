using Datalayer.Data;
using Datalayer.Enumerations;
using Datalayer.Models;
using DataLayer.Models.Base;
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
            => await NotRemoved
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.User.ChatId == chatid);

        public async Task<Request> GetRequestByUserIdAsync(Guid userid)
            => await NotRemoved
            .FirstOrDefaultAsync(x => x.UserId == userid);

        public async Task<bool> OpenRequestExistsAsync(Guid userId)
            => await NotRemoved
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.RequestStatus == RequestStatus.New);

        public override async Task<List<Request>> GetAllAsync(int skip, int take)
            => await Entities.Skip(skip).Take(take).OrderByDescending(x=>x.CreatedAt).ToListAsync();

        public async Task<List<Request>> SuggestAsync(Request request)
            => await Entities
            .Include(x => x.User)
            .Where(x=>x.RequestType != request.RequestType &&
            (x.Source == request.Destination || x.Destination == request.Source) &&
            (request.RequestType == RequestType.FreightOwner ? x.RequestType == RequestType.Passenger && x.FlightDate < DateTime.Now : true))
            .ToListAsync();
    }
}
