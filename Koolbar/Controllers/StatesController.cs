using Datalayer.Models;
using Koolbar.Dtos;
using Koolbar.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Koolbar.Controllers
{
    [Route("api/states")]
    [ApiController]
    public class StatesController : Controller
    {
        private readonly IStateRepository _stateRepository;

        public StatesController(IStateRepository stateRepository)
        {
            _stateRepository = stateRepository;
        }

        [HttpGet("search/{q}")]
        public async Task<ActionResult<List<CityDto>>> Index([FromRoute] string q)
        {
            try
            {
                var city = await _stateRepository.SearchAsync(q);
                return city.Select(x => new CityDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    PersianTitle = x.PersianTitle,
                    CountryTitle = x.State.Title,
                    StateTitle = x.State.Country.Title,
                    StateId = x.StateId,
                    Lat = x.Lat,
                    Long = x.Long
                }).ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
