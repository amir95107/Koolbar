using Datalayer.Models;

namespace Koolbar.Repositories
{
    public interface IUserRepository: IBaseRepository<User, Guid>
    {
        Task<User> GetUserByChatId(long chatId);
        Task<List<User>> GetUsersAsync(string? username, long? chatId, int take, int skip);
    }
}
