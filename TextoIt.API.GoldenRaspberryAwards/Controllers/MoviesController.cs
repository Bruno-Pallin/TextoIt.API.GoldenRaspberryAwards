using Microsoft.AspNetCore.Mvc;
using TextoIt.API.GoldenRaspberryAwards.Models;

namespace TextoIt.API.GoldenRaspberryAwards.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly ILogger<MoviesModel> _logger;

        public MoviesController(ILogger<MoviesModel> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Produces("application/json")]
        [Consumes("application/json")]
        public string GetData()
        {
            return "TestGet";
        }

        [HttpPost]
        [Produces("application/json")]
        [Consumes("application/json")]
        public string CreateData()
        {
            return "TestPost";
        }

        [HttpPut]
        [Produces("application/json")]
        [Consumes("application/json")]
        public string UpdateData()
        {
            return "TestPut";
        }

        [HttpPatch]
        [Produces("application/json")]
        [Consumes("application/json")]
        public string UpdatePartialData()
        {
            return "TestePatch";
        }

        [HttpDelete]
        [Produces("application/json")]
        [Consumes("application/json")]
        public string DeleteData()
        {
            return "TestDelete";
        }
    }
}
