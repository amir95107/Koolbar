using Datalayer.Models;
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
        public async Task<List<City>> Index([FromRoute] string q)
        {
            try
            {
                return await _stateRepository.SearchAsync(q);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
