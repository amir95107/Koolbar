//using Datalayer.Models;
//using Koolbar.Dtos;
//using Koolbar.Repositories;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace Koolbar.Controllers
//{
//    [Route("user")]
//    public class UsersController : ControllerBase
//    {
//        private readonly UserManager<User> UserManager;
//        private readonly IUserRepository _userRepository;

//        public UsersController(UserManager<User> userManager,
//            IUserRepository userRepository)
//        {
//            UserManager = userManager;
//            _userRepository = userRepository;
//        }

//        public async Task<IActionResult> Index(string? username, long? chatId, int page = 1)
//            => Ok(await _userRepository.GetUsersAsync(username, chatId, 20, (page - 1) * 20));

//        public async Task<IActionResult> BanOrUnbanUserAsync(Guid id, bool isBanned)
//        {
//            var user = await _userRepository.FindAsync(id);
//            if (user == null)
//            {
//                return NotFound();
//            }
//            user.IsBanned = isBanned;
//            user.BannedAt = DateTime.Now;
//            await _userRepository.SaveChangesAsync();
//            return NoContent();
//        }

//        public async Task<IActionResult> VerifyOrUnVerifyUserAsync(Guid id, bool isVerified)
//        {
//            var user = await _userRepository.FindAsync(id);
//            if (user == null)
//            {
//                return NotFound();
//            }
//            user.IsVerified = isVerified;
//            user.VerifiedAt = DateTime.Now;
//            await _userRepository.SaveChangesAsync();
//            return NoContent();
//        }
//    }
//}
