using Datalayer.Enumerations;
using Datalayer.Models;
using Koolbar.Dtos;
using Koolbar.Repositories;
using Koolbar.Services;
using KoolbarTelegramBot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace Koolbar.Controllers
{
    [Route("requests")]
    public class RequestsController : Controller
    {
        private readonly IRequestRepository _requestRepository;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> UserManager;

        public RequestsController(
            IRequestRepository requestRepository,
            UserManager<User> userManager,
            IUserRepository userRepository)
        {
            _requestRepository = requestRepository;
            UserManager = userManager;
            _userRepository = userRepository;
        }


        [Authorize]
        [HttpGet("all")]
        public async Task<IActionResult> Index(RequestType? type, string? source, string? destination, int take = 10, int p = 1)
        {
            var skip = (p - 1) * take;
            var count = await _requestRepository.CountAsync();
            ViewBag.PageCount = count / take + 1;

            return View(await _requestRepository.GetAllRequestsAsync(skip, take));
        }

        // GET: Requests/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _requestRepository.FindAsync(id.Value);
            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // GET: Requests/Create
        public IActionResult Main()
        {
            return View();
        }

        [HttpGet("create/{username}")]
        // GET: Requests/Create
        public IActionResult Create([FromRoute] long username)
        {
            //var user = await _userRepository.GetUserByChatId(username);
            //ViewBag.User = user;
            //var sources = await _requestRepository.MostPopularCitiesAsync(Services.LocationType.Source);
            //var destionations = await _requestRepository.MostPopularCitiesAsync(Services.LocationType.Destionation);

            //ViewBag.MostPopularCities = new[] { sources, destionations };
            return View();
        }

        // POST: Requests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, and validate
        // them if needed. See https://go.microsoft.com/fwlink/?LinkId=317598 for details.
        [HttpPost("create")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] RequestsDto request)
        {
            if (ModelState.IsValid)
            {
                var user = await _userRepository.GetUserByChatId(request.ChatId);
                if (user is null)
                {
                    throw new Exception("User not found!");
                }

                var req = new Request
                {
                    UserId = user.Id,
                    Description = request.Description,
                    Destination = request.Destination,
                    FlightDate = request.FlightDate,
                    RequestStatus = request.RequestType == RequestType.Passenger ? RequestStatus.FlightDateDeclared : RequestStatus.DescriptionDeclared,
                    Source = request.Source,
                    RequestType = request.RequestType,
                    SourceCityId = request.SourceCityId != null ? Guid.Parse(request.SourceCityId) : null,
                    DestinationCityId = request.DestinationCityId != null ? Guid.Parse(request.DestinationCityId) : null
                };
                await _requestRepository.AddAsync(req);
                await _requestRepository.SaveChangesAsync();

                var message = GenerateTextMessage(request);

                await Notification.SendMessage(message);

                return Redirect($"/requests/Suggest/{req.Id}");
            }
            return View(request);
        }

        [HttpGet("suggest/{id}")]
        public async Task<ActionResult<List<RequestDto>>> Suggest(Guid id)
        {
            var request = await _requestRepository.FindAsync(id);
            if (request is null)
                throw new Exception("Request not found!");
            var suggests = await _requestRepository.SuggestAsync(request);

            ViewBag.CurrentRequest = request;
            return View("suggests", suggests);
        }

        private string GenerateTextMessage(RequestDto req)
        {
            var typeTxt = req.RequestType == RequestType.Passenger ? "#مسافر" : "#بار";
            var date = req.RequestType == RequestType.Passenger ? req.FlightDate.ToString() + "\n" : "";

            return $"<a href='https://t.me/{req.Username}'>@{req.Username}</a> \n" +
                $"{typeTxt}\n" +
                $"مبدا: {req.Source}\n" +
                $"مقصد: {req.Destination}\n" +
                $"{date}" +
                $"{req.Description}";
        }

        private string GenerateTextMessage(RequestsDto req)
        {
            var typeTxt = req.RequestType == RequestType.Passenger ? "#مسافر" : "#بار";
            var date = req.RequestType == RequestType.Passenger ? req.FlightDate.ToString() + "\n" : "";

            return $"<a href='https://t.me/{req.Username}'>@{req.Username}</a> \n" +
                $"{typeTxt}\n" +
                $"مبدا: {req.Source}\n" +
                $"مقصد: {req.Destination}\n" +
                $"{date}" +
                $"{req.Description}";
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _requestRepository.FindAsync(id.Value);
            if (request == null)
            {
                return NotFound();
            }

            var user = await UserManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
                return BadRequest("Username not found");

            var req = new RequestDto
            {
                UserId = user.Id,
                Description = request.Description,
                Destination = new CityDto { Title = request.Destination },
                FlightDate = request.FlightDate,
                LimitDate = request.LimitDate,
                RequestStatus = request.RequestType == RequestType.Passenger ? RequestStatus.FlightDateDeclared : RequestStatus.DescriptionDeclared,
                Source = new CityDto { Title = request.Source },
                RequestType = request.RequestType
            };
            return View(req);
        }

        // POST: Requests/Edit/5 (cont.)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,UserId,RequestType,Description,Source,Destination,FlightDate,LimitDate,RequestStatus")] RequestDto request)
        {
            if (id != request.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var req = await _requestRepository.FindAsync(id);
                    req.LimitDate = request.LimitDate;
                    req.FlightDate = request.FlightDate;
                    req.Description = request.Description;
                    req.Destination = request.Destination.Title;
                    req.Source = request.Source.Title;
                    req.RequestType = request.RequestType;

                    _requestRepository.Modify(req);
                    await _requestRepository.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await RequestExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ViewBag.Error = "One or more fields are not filled!";
            }
            return View(request);
        }

        // GET: Requests/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _requestRepository.FindAsync(id.Value);
            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // POST: Requests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var request = await _requestRepository.FindAsync(id);
            _requestRepository.Remove(request);
            await _requestRepository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> RequestExists(Guid id)
        {
            return await _requestRepository.ExistsAsync(id);
        }

    }
}
