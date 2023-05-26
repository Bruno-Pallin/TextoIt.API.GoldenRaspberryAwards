using Microsoft.AspNetCore.Mvc;
using TextoIt.API.GoldenRaspberryAwards.Models;
using TextoIt.API.GoldenRaspberryAwards.Repository;

namespace TextoIt.API.GoldenRaspberryAwards.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly ILogger<MoviesModel> _logger;
        private readonly MoviesDAO _moviesDAO;

        public MoviesController(ILogger<MoviesModel> logger)
        {
            _logger = logger;
            string? dbFilePath = System.Environment.GetEnvironmentVariable("DBFilePath");
            if (dbFilePath == null) throw new NullReferenceException("Please, configure the DBFilePath in launchSettings.json");
            _moviesDAO = new MoviesDAO(dbFilePath);
        }

        [HttpGet]
        [Route("GreaterAndSmallestWinRange")]
        [Consumes("application/json")]
        public string GetWorstMovies()
        {
            return "Teste";
        }

        [HttpGet]
        [Consumes("application/json")]
        public IActionResult GetMovie([FromBody] MoviesModel movie)
        {
            try
            {
                if (movie.year < 1850 ||
                    String.IsNullOrEmpty(movie.title))
                    return BadRequest("Please, to update a movie, you need to fill the mandatory fields: year (higher than 1850) and title.");

                string selectQuery = $"SELECT * FROM MoviesGoldenRaspberryAwards WHERE year={movie.year} AND title='{movie.title}'";
                List<MoviesModel> movieReturned = _moviesDAO.Read(selectQuery);

                IActionResult response;
                if (movieReturned.Count > 0)
                {
                    response = Ok(movieReturned[0]);
                }
                else
                {
                    response = NotFound($"Movie not found in database.");
                }

                return response;
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost]
        [Consumes("application/json")]
        public IActionResult CreateMovie([FromBody] MoviesModel movie)
        {
            try
            {
                if (movie.year < 1850 ||
                    String.IsNullOrEmpty(movie.title) ||
                    String.IsNullOrEmpty(movie.producers) ||
                    String.IsNullOrEmpty(movie.studio) ||
                    movie.winner == null &&
                    (movie.winner != "yes" && movie.winner != ""))
                    return BadRequest("Please, to create a movie, you need to fill the fields: year (need to be greather than 1850), title, studio, producers and winner. \n winner can only be 'yes' or empty ''");

                int httpReturnStatus = _moviesDAO.Create(movie);

                IActionResult response = Created("MoviesDB", movie);

                if (httpReturnStatus == 409) response = Conflict($"Movie already exists in database.");

                return response;
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPut]
        [Consumes("application/json")]
        public IActionResult UpdateMovie([FromBody] MoviesModel movie)
        {
            try
            {
                if (movie.year < 1850 ||
                    String.IsNullOrEmpty(movie.title) ||
                    String.IsNullOrEmpty(movie.producers) ||
                    String.IsNullOrEmpty(movie.studio) ||
                    movie.winner == null &&
                    (movie.winner != "yes" && movie.winner != ""))
                    return BadRequest("Please, to update a movie, you need to fill the fields: year (need to be higher than 1850), title, studio, producers and winner. \n winner can only be 'yes' or empty ''");

                int httpReturnStatus = _moviesDAO.Update(movie);

                IActionResult response = NoContent();
                if (httpReturnStatus == 404) response = NotFound($"Movie not found in database.");

                return response;
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPatch]
        [Consumes("application/json")]
        public IActionResult UpdatePartialMovie([FromBody] MoviesModel movie)
        {
            try
            {
                if (movie.year < 1850 ||
                    String.IsNullOrEmpty(movie.title))
                    return BadRequest("Please, to update a movie, you need to fill the mandatory fields: year (higher than 1850) and title.");

                if (String.IsNullOrEmpty(movie.studio) &&
                    String.IsNullOrEmpty(movie.producers) &&
                    movie.winner == null)
                    return BadRequest("Please, to update a movie, you need to fill at least one optional field: studio, producers or winner.");

                int httpReturnStatus = _moviesDAO.Update(movie);

                if (httpReturnStatus == 404) return NotFound($"Movie not found in database.");

                string selectQuery = $"SELECT * FROM MoviesGoldenRaspberryAwards WHERE year={movie.year} AND title='{movie.title}'";
                List<MoviesModel> movieUpdated = _moviesDAO.Read(selectQuery);
                return Ok(movieUpdated[0]);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete]
        [Consumes("application/json")]
        public IActionResult DeleteMovie([FromBody] MoviesModel movie)
        {
            try
            {
                if (movie.year < 1850 ||
                    String.IsNullOrEmpty(movie.title))
                    return BadRequest("Please, to update a movie, you need to fill the mandatory fields: year (higher than 1850) and title.");

                int httpReturnStatus = _moviesDAO.Delete(movie.year, movie.title);

                IActionResult response = NoContent();
                if (httpReturnStatus == 404) response = NotFound($"Movie not found in database.");
                return response;
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
