using Datalayer.Data;
using Datalayer.Enumerations;
using Datalayer.Models;
using DataLayer.Models.Base;
using Koolbar.Dtos;
using Koolbar.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Telegram.Bot.Types;

namespace Koolbar.Services
{
    public class RequestRepository : BaseRepository<Request, Guid>, IRequestRepository
    {
        private readonly IQueryable<Request> NotRemoved;
        private readonly DbSet<City> _cities;
        public RequestRepository(IHttpContextAccessor accessor, ApplicationDBContext context) : base(accessor, context)
        {
            NotRemoved = Entities.Where(x => x.RemovedAt == null);
            _cities = context.Set<City>();
        }

        public async Task<Request> GetRequestByChatIdAsync(long chatid)
        {
            try
            {
                return await NotRemoved
                .Include(x => x.User)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(x => x.User.ChatId == chatid && !x.IsCompleted);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<List<Request>> GetRequestsByChatIdAsync(long chatid)
        {
            try
            {
                return await NotRemoved
                .Include(x => x.User)
                .OrderByDescending(x => x.CreatedAt)
                .Where(x => x.User.ChatId == chatid && !x.IsCompleted)
                .ToListAsync();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<Request> GetRequestByUserIdAsync(Guid userid)
            => await NotRemoved
            .FirstOrDefaultAsync(x => x.UserId == userid);

        public async Task<bool> OpenRequestExistsAsync(Guid userId)
            => await NotRemoved
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.RequestStatus == RequestStatus.New);

        public async Task<List<RequestDto>> GetAllRequestsAsync(int skip, int take)
            => await Entities
            .Include(x => x.User)
            .Skip(skip)
            .Take(take)
            .Select(x => new RequestDto
            {
                Id = x.Id,
                ChatId = x.User.ChatId,
                UserId = x.UserId,
                RequestStatus = x.RequestStatus,
                Description = x.Description,
                Destination = new CityDto { Title = x.Destination },
                FlightDate = x.FlightDate,
                LimitDate = x.LimitDate,
                RequestType = x.RequestType,
                Source = new CityDto { Title = x.Source },
                Username = x.User.UserName
            })
            .ToListAsync();

        public async Task<List<RequestDto>> GetAllRequestsAsync(RequestType? type, string? source, string? destination, int take = 10, int p = 1)
            => await Entities
            .Include(x => x.User)
            .Where(x => type != null ? x.RequestType == type : true &&
                        !string.IsNullOrWhiteSpace(source) ? x.Source == source : true &&
                        !string.IsNullOrWhiteSpace(destination) ? x.Destination == destination : true)
            .Skip((p - 1) * take)
            .Take(take)
            .Select(x => new RequestDto
            {
                Id = x.Id,
                ChatId = x.User.ChatId,
                UserId = x.UserId,
                RequestStatus = x.RequestStatus,
                Description = x.Description,
                Destination = new CityDto { Title = x.Destination },
                FlightDate = x.FlightDate,
                LimitDate = x.LimitDate,
                RequestType = x.RequestType,
                Source = new CityDto { Title = x.Source },
                Username = x.User.UserName
            })
            .ToListAsync();

        public async Task<List<RequestDto>> SuggestAsync(Request request)
        {
            var query = NotRemoved
                .Include(x => x.User)
                .AsNoTracking()
                .Where(x => x.CreatedAt > DateTime.Now.AddDays(-30) 
                && (request.RequestType == RequestType.FreightOwner ? x.RequestType == RequestType.Passenger && x.FlightDate > DateTime.Now : true)
                && x.RequestType != request.RequestType && x.UserId != request.UserId);

            var req = await query
            .Where(x => 
            (x.Source == request.Source && x.Destination == request.Destination))
            .Select(x => new RequestDto
            {
                Id = x.Id,
                ChatId = x.User.ChatId,
                RequestType = x.RequestType,
                Description = x.Description,
                Destination = new CityDto { Title = x.Destination, UniqueKey = x.DestinationCityUniqueKey.GetValueOrDefault() },
                FlightDate = x.FlightDate,
                LimitDate = x.LimitDate,
                Key = x.Key,
                MessageId = x.MessageId,
                RequestStatus = x.RequestStatus,
                Source = new CityDto { Title = x.Source, UniqueKey = x.SourceCityUniqueKey.GetValueOrDefault() },
                UserId = x.UserId,
                Username = x.User.UserName,

            })
            .ToListAsync();

            if (req.Count < 5)
            { //equal destination in sources's state
                var stateCities = await _cities
                    .Include(x => x.State)
                    .AsNoTracking()
                    .Where(x => x.UniqueKey == request.SourceCityUniqueKey)
                    .SelectMany(x => x.State.Cities)
                    .ToListAsync();

                var current = stateCities
                    .FirstOrDefault(x => x.UniqueKey == request.SourceCityUniqueKey);
                
                if (current is null)
                    return null;

                var stateCitiesKeys = stateCities
                    .OrderBy(x => 
                    CalculateDistance(current.Lat.GetValueOrDefault(),
                                      current.Long.GetValueOrDefault(),
                                      x.Lat.GetValueOrDefault(),
                                      x.Long.GetValueOrDefault()))
                    .Select(x => x.UniqueKey)
                    .ToList();


                req.AddRange(await query
            .Where(x =>
            x.DestinationCityUniqueKey == request.DestinationCityUniqueKey && stateCitiesKeys.Contains(x.SourceCityUniqueKey!.Value))
            .Select(x => new RequestDto
            {
                Id = x.Id,
                ChatId = x.User.ChatId,
                RequestType = x.RequestType,
                Description = x.Description,
                Destination = new CityDto { Title = x.Destination },
                FlightDate = x.FlightDate,
                LimitDate = x.LimitDate,
                Key = x.Key,
                MessageId = x.MessageId,
                RequestStatus = x.RequestStatus,
                Source = new CityDto { Title = x.Source, UniqueKey = x.SourceCityUniqueKey },
                UserId = x.UserId,
                Username = x.User.UserName
            })
            .Take(20)
            .ToListAsync());
            
            }

            if (req.Count < 5)
            {
                var stateCities = await _cities
                    .Include(x => x.State)
                    .AsNoTracking()
                    .Where(x => x.UniqueKey == request.DestinationCityUniqueKey)
                    .SelectMany(x => x.State.Cities)
                    .ToListAsync();

                var current = stateCities.FirstOrDefault(x => x.UniqueKey == request.DestinationCityUniqueKey);
                if (current is null)
                    return null;

                var stateCitiesKeys = stateCities
                    .OrderBy(x => CalculateDistance(current.Lat.GetValueOrDefault(), current.Long.GetValueOrDefault(), x.Lat.GetValueOrDefault(), x.Long.GetValueOrDefault()))
                    .Select(x => x.UniqueKey)
                    .ToList();

                req.AddRange(await query
            .Where(x => x.Source == request.Source && stateCitiesKeys.Contains(x.DestinationCityUniqueKey!.Value))
            .Select(x => new RequestDto
            {
                Id = x.Id,
                ChatId = x.User.ChatId,
                RequestType = x.RequestType,
                Description = x.Description,
                Destination = new CityDto { Title = x.Destination, UniqueKey = x.DestinationCityUniqueKey },
                FlightDate = x.FlightDate,
                LimitDate = x.LimitDate,
                Key = x.Key,
                MessageId = x.MessageId,
                RequestStatus = x.RequestStatus,
                Source = new CityDto { Title = x.Source, UniqueKey = x.SourceCityUniqueKey },
                UserId = x.UserId,
                Username = x.User.UserName
            })
            .Take(20)
            .ToListAsync());
            }

            return req;
        }

        public async Task<Request> GetCompleteRequestByChatIdAsync(long chatid)
            => await NotRemoved
                .Include(x => x.User)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(x => x.User.ChatId == chatid && x.IsCompleted);

        public async Task<int?> GetLastKeyAsync()
            => await NotRemoved
            .Select(x => x.Key)
            .MaxAsync();

        public async Task<Request> GetRequestByKeyAsync(int key)
            => await NotRemoved
            .FirstOrDefaultAsync(x => x.Key == key);


        private double CalculateDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Pow(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2), 0.5);
        }


        public async Task<List<PopularCity>> MostPopularCitiesAsync(LocationType type)
            => await NotRemoved
            .GroupBy(x => type == LocationType.Source ? x.Source : x.Destination)
            .OrderByDescending(x => x.Count())
            .Select(x => new PopularCity { City = x.Key, Count = x.Count() })
            .Take(10)
            .ToListAsync();


    }

    public enum LocationType
    {
        Source, Destionation
    }

    public class PopularCity
    {
        public Guid CityId { get; set; }
        public string City { get; set; }
        public int Count { get; set; }
    }
}
