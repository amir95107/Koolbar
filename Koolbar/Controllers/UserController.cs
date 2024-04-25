using Datalayer.Models;
using Koolbar.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Koolbar.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> UserManager;

        public UserController(UserManager<User> userManager)
        {
            UserManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync0(UserDto user)
        {
            await UserManager.CreateAsync(new User
            {
                ChatId = user.ChatId
            });

            return (IActionResult)Task.CompletedTask;
        }

        [HttpGet("chatids")]
        public async Task<ActionResult<List<long>>> GetAllChatIds()
        {
            return await UserManager
                .Users
                .Where(x => !x.MuteNotification)
                .Select(x => x.ChatId)
                .ToListAsync();
        }
    }
}
