using Datalayer.Data;
using Datalayer.Models;
using Koolbar.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Koolbar.Services
{
    public class UserRepository : BaseRepository<User, Guid>, IUserRepository
    {
        public UserRepository(IHttpContextAccessor accessor, ApplicationDBContext context) : base(accessor, context)
        {
        }

        public async Task<User> GetUserByChatId(long chatId)
            => await Entities.FirstOrDefaultAsync(x=>x.ChatId == chatId);

        public async Task<List<User>> GetUsersAsync(string? username, long? chatId, int take, int skip)
            => await Entities
            .Where(x=> !string.IsNullOrEmpty(username) ? x.UserName == username : true &&
                        chatId != null ? x.ChatId == chatId : true)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }
}
