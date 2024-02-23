using Datalayer.Data;
using Datalayer.Models;
using Koolbar.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Koolbar.Services
{
    public class StateRepository : BaseRepository<State, Guid>, IStateRepository
    {
        public StateRepository(IHttpContextAccessor accessor, ApplicationDBContext context) : base(accessor, context)
        {
        }

        public async Task<List<City>> SearchAsync(string q)
            => await Entities
             .Include(x => x.Country)
             .Include(x => x.Cities)
             .SelectMany(x => x.Cities)
             .Where(x => x.Title.Contains(q) || (!string.IsNullOrEmpty(x.PersianTitle) && x.PersianTitle.Contains(q)))
             .Take(20)
             .OrderBy(x => x.Title)
             .ToListAsync();
    }
}
