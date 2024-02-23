using Koolbar.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Koolbar.Controllers
{
    public class RequestsController : Controller
    {
        private readonly IRequestRepository _requestRepository;

        public RequestsController(IRequestRepository requestRepository)
        {
            _requestRepository = requestRepository;
        }

        public async Task<IActionResult> Index(int p = 1, int take = 10)
        {
            var skip = (p - 1) * take;
            return View(await _requestRepository.GetAllAsync(skip, take));
        }
    }
}
