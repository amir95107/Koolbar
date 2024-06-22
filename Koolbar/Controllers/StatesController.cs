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
                    Title = x.Title,
                    PersianTitle = x.PersianTitle,
                    State = new StateDto
                    {
                        Title = x.State.Title,
                        PersianTitle = x.State.PersianTitle,
                        Country = new CountryDto
                        {
                            PersianTitle = x.State.Country.PersianTitle,
                            Title = x.State.Country.Title,
                            Emoji = x.State.Country.Emoji
                        }
                    }
                }).ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            try
            {
                var city = await _stateRepository.SearchAsync(q);
                var result = city.Select(x => new
                {
                    id = x.Id,
                    text = $"{x.Title} - {x.State.Title} - {x.State.Country.Title} - {x.State.Country.Emoji}"
                }).ToList();
                return Ok(new { results = result });
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
