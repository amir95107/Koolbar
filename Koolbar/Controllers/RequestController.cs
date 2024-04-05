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

        [HttpGet("{chatId}")]
        public async Task<RequestDto> GetCurrentRequest(long chatId)
        {
            var existingRequest = await _requestRepository.GetRequestByChatIdAsync(chatId);

            if (existingRequest == null)
                return null;

            return new RequestDto
            {
                RequestStatus = existingRequest.RequestStatus,
                Description = existingRequest.Description,
                Destination = existingRequest.Destination,
                FlightDate = existingRequest.FlightDate,
                RequestType = existingRequest.RequestType,
                Source = existingRequest.Source,
                UserId = existingRequest.UserId
            };
        }

        // GET: api/<RequestsController>
        [HttpPost]
        public async Task<ActionResult<RequestDto>> AddRequest([FromBody] RequestDto request)
        {
            var user = await UserManager.FindByNameAsync(request.Username.ToUpper());

            if (user is null)
            {
                var u = await UserManager.CreateAsync(new User
                {
                    UserName = request.Username,
                    ChatId = request.ChatId
                });

                user = await UserManager.FindByNameAsync(request.Username);
            }

            var existingRequest = await _requestRepository.GetRequestByChatIdAsync(request.ChatId);

            if (existingRequest is null)
                await _requestRepository.AddAsync(new Request
                {
                    UserId = user.Id
                });
            else
                return new RequestDto
                {
                    RequestStatus = existingRequest.RequestStatus,
                    ChatId = request.ChatId,
                    Username = request.Username,
                    Description = existingRequest.Description,
                    Destination = existingRequest.Destination,
                    FlightDate = existingRequest.FlightDate,
                    RequestType = existingRequest.RequestType,
                    Source = existingRequest.Source,
                    UserId = existingRequest.UserId
                };

            await _requestRepository.SaveChangesAsync();

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
                if (request.RequestType == RequestType.FreightOwner)
                    existiongRequest.IsCompleted = true;

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
                existiongRequest.IsCompleted = true;
                _requestRepository.Modify(existiongRequest);
                await _requestRepository.SaveChangesAsync();
            }
        }

        [HttpGet("suggest/{id}")]
        public async Task<ActionResult<List<RequestDto>>> Suggest([FromRoute] long id)
        {
            try
            {
                var existiongRequest = await _requestRepository.GetCompleteRequestByChatIdAsync(id);
                if (existiongRequest == null)
                    return new List<RequestDto>();

                return await _requestRepository.SuggestAsync(existiongRequest);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost("all")]
        public async Task<ActionResult<RequestDto>> AddAllRequest([FromBody] RequestDto request)
        {
            var user = await UserManager.FindByNameAsync(request.Username.ToUpper());

            if (user is null)
            {
                var u = await UserManager.CreateAsync(new User
                {
                    UserName = request.Username,
                    ChatId = request.ChatId
                });

                user = await UserManager.FindByNameAsync(request.Username);
            }

            var existingRequest = await _requestRepository.GetRequestsByChatIdAsync(request.ChatId);

            //if (existingRequest is null || existingRequest.Count == 0)
            //{
            //    await _requestRepository.AddAsync(new Request
            //    {
            //        UserId = user.Id,
            //        Description = request.Description,
            //        Destination = request.Destination,
            //        FlightDate = request.FlightDate,
            //        LimitDate = request.LimitDate,
            //        RequestStatus = request.RequestStatus,
            //        RequestType = request.RequestType,
            //        Source = request.Source,
            //        IsCompleted = true
            //    });
            //    await _requestRepository.SaveChangesAsync();
            //}
            //else
            //{
            //    request.Requests = existingRequest
            //        .Select(x => new RequestDto
            //        {
            //            RequestStatus = x.RequestStatus,
            //            ChatId = request.ChatId,
            //            Username = request.Username,
            //            Description = x.Description,
            //            Destination = x.Destination,
            //            FlightDate = x.FlightDate,
            //            RequestType = x.RequestType,
            //            Source = x.Source,
            //            UserId = x.UserId
            //        })
            //    .ToList();
            //}

            var key = await _requestRepository.GetLastKeyAsync();

            await _requestRepository.AddAsync(new Request
            {
                UserId = user.Id,
                Description = request.Description,
                Destination = request.Destination,
                FlightDate = request.FlightDate,
                LimitDate = request.LimitDate,
                RequestStatus = request.RequestStatus,
                RequestType = request.RequestType,
                Source = request.Source,
                IsCompleted = true,
                MessageId = request.MessageId,
                Key = key.Value + 1,
            });
            await _requestRepository.SaveChangesAsync();

            request.Key = key.Value + 1;

            return request;
        }
    }
}
