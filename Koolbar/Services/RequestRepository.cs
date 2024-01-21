using Datalayer.Data;
using Datalayer.Enumerations;
using Datalayer.Models;
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
    }
}
