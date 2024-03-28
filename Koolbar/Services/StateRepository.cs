using Datalayer.Data;
using Datalayer.Models;
using Koolbar.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Koolbar.Services
{
    public class StateRepository : BaseRepository<State, Guid>, IStateRepository
    {
        private readonly DbSet<City> _cities;
        public StateRepository(IHttpContextAccessor accessor, ApplicationDBContext context) : base(accessor, context)
        {
            _cities = context.Set<City>();
        }

        //public async Task<List<City>> SearchAsync(string q)
        //    => await Entities
        //     .Include(x => x.Country)
        //     .Include(x => x.Cities)
        //     .SelectMany(x => x.Cities)
        //     .Where(x => x.Title.Contains(q))
        //     .Take(10)
        //     .OrderBy(x => x.Title)
        //     .ToListAsync();

        public async Task<List<City>> SearchAsync(string q)
           => await _cities
            .Include(x=>x.State)
            .ThenInclude(x=>x.Country)
            .Where(x=>x.Title.Contains(q))
            //.DistinctBy(x=>x.Title)
            .Take(10)
            .OrderBy(x => x.Title)
            .ToListAsync();
    }
}
