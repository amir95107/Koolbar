using Datalayer.Models;

namespace Koolbar.Repositories
{
    public interface IStateRepository:IBaseRepository<State,Guid>
    {
        Task<List<City>> SearchAsync(string q);
    }
}
