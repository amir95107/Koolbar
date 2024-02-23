using Datalayer.Enumerations;
using Datalayer.Models;
using Koolbar.Dtos;
using Koolbar.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Koolbar.Controllers
{
    [Route("api/requests")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly IRequestRepository _requestRepository;
        private readonly UserManager<User> UserManager;

        public RequestController(
            IRequestRepository requestRepository,
            UserManager<User> userManager)
        {
            _requestRepository = requestRepository;
            UserManager = userManager;
        }

        // GET: api/<RequestsController>
        [HttpPost]
        public async Task<ActionResult<RequestDto>> AddRequest([FromBody] RequestDto request)
        {
            var user = await UserManager.FindByNameAsync(request.ChatId.ToString());

            if (user is null)
            {
                var u = await UserManager.CreateAsync(new User
                {
                    UserName = request.ChatId.ToString(),
                    ChatId = request.ChatId
                });

                user = await UserManager.FindByNameAsync(request.ChatId.ToString());
            }

            var openRequestExists = await _requestRepository.OpenRequestExistsAsync(user.Id);

            if (!openRequestExists)
                await _requestRepository.AddAsync(new Request
                {
                    UserId = user.Id
                });
            else
                return StatusCode(422, new RequestDto
                {
                    ErrorMessage = "درخواست تکمیل نشده دیگری وجود دارد."
                });

            try
            {
                await _requestRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                throw;
            }

            return request;
        }

        [HttpPost("type")]

        public async Task AddRequestType([FromBody] RequestDto request)
        {
            var existiongRequest = await _requestRepository.GetRequestByChatIdAsync(request.ChatId);
            if (existiongRequest != null)
            {
                existiongRequest.RequestType = request.RequestType;
                existiongRequest.RequestStatus = RequestStatus.TypeDeclared;
                _requestRepository.Modify(existiongRequest);
                await _requestRepository.SaveChangesAsync();

            }
        }

        [HttpPost("source")]
        public async Task AddSource([FromBody] RequestDto request)
        {
            var existiongRequest = await _requestRepository.GetRequestByChatIdAsync(request.ChatId);
            if (existiongRequest != null && existiongRequest.RequestType != null)
            {
                existiongRequest.Source = request.Source;
                existiongRequest.RequestStatus = RequestStatus.SourceDeclared;
                _requestRepository.Modify(existiongRequest);
                await _requestRepository.SaveChangesAsync();
            }
        }

        [HttpPost("destination")]
        public async Task AddDestination([FromBody] RequestDto request)
        {
            var existiongRequest = await _requestRepository.GetRequestByChatIdAsync(request.ChatId);
            if (existiongRequest != null &&
                existiongRequest.RequestType != null &&
                existiongRequest.Source != null)
            {
                existiongRequest.Destination = request.Destination;
                existiongRequest.RequestStatus = RequestStatus.DestinationDeclared;
                _requestRepository.Modify(existiongRequest);
                await _requestRepository.SaveChangesAsync();
            }
        }
       
        [HttpPost("description")]
        public async Task AddDescription([FromBody] RequestDto request)
        {
            var existiongRequest = await _requestRepository.GetRequestByChatIdAsync(request.ChatId);
            if (existiongRequest != null &&
                existiongRequest.RequestType != null &&
                existiongRequest.Source != null &&
                existiongRequest.Destination != null)
            {
                existiongRequest.Description = request.Description;
                existiongRequest.RequestStatus = RequestStatus.DescriptionDeclared;
                _requestRepository.Modify(existiongRequest);
                await _requestRepository.SaveChangesAsync();
            }
        }

        [HttpPost("flightdate")]
        public async Task AddFlightDate([FromBody] RequestDto request)
        {
            var existiongRequest = await _requestRepository.GetRequestByChatIdAsync(request.ChatId);
            if (existiongRequest != null &&
                existiongRequest.RequestType != null &&
                existiongRequest.Source != null &&
                existiongRequest.Destination != null && 
                existiongRequest.Description != null)
            {
                existiongRequest.FlightDate = request.FlightDate;
                existiongRequest.RequestStatus = RequestStatus.FlightDateDeclared;
                _requestRepository.Modify(existiongRequest);
                await _requestRepository.SaveChangesAsync();
            }
        }

        [HttpGet("suggest/{id}")]
        public async Task<List<Request>> Suggest([FromRoute] long id)
        {
            var existiongRequest = await _requestRepository.GetRequestByChatIdAsync(id);
            if (existiongRequest != null &&
                existiongRequest.RequestType != null &&
                existiongRequest.Source != null &&
                existiongRequest.Destination != null &&
                existiongRequest.Description != null)
            {
                return await _requestRepository.SuggestAsync(existiongRequest);
            }
            throw new Exception("Method not allowed");
        }
    }
}
