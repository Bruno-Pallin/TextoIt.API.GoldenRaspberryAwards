using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
            string? dbName = System.Environment.GetEnvironmentVariable("DBName");
            if (dbFilePath == null) throw new NullReferenceException("Please, configure the DBFilePath in launchSettings.json");
            if (dbName == null) throw new NullReferenceException("Please, configure the DBName in launchSettings.json");

            _moviesDAO = new MoviesDAO(dbFilePath, dbName);
        }

        [HttpGet]
        [Route("GreaterAndSmallestWinRange")]
        [Consumes("application/json")]
        public string GetWorstMovies()
        {
            string sqlGetQuery = string.Format("SELECT * FROM {0} WHERE winner=\"yes\" ORDER BY year ASC", _moviesDAO.dbName);
            List<MoviesModel> winnerMoviesList = _moviesDAO.Read(sqlGetQuery);
            List<string> producersList = new List<string>();

            foreach (MoviesModel movie in winnerMoviesList)
            {
                if (movie.producers != null)
                {
                    string[] delimiters = new[] { ",", "and" };
                    string[] producers = movie.producers.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string producer in producers)
                    {
                        if (!producersList.Contains(producer.Trim())) producersList.Add(producer.Trim());
                    }
                }
            }
            return "Teste";
        }


        [HttpGet]
        [Route("GetAllMovies")]
        [Produces("application/json")]
        public IActionResult GetAllMovies()
        {
            try
            {
                string selectQuery;

                selectQuery = string.Format("SELECT * FROM {0}", _moviesDAO.dbName);

                List<MoviesModel> movieReturned = _moviesDAO.Read(selectQuery);

                IActionResult response;
                if (movieReturned.Count > 0)
                {
                    response = Ok(JsonConvert.SerializeObject(movieReturned));
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

        [HttpGet]
        [Route("GetMovie")]
        [Produces("application/json")]
        public IActionResult GetMovie()
        {
            try
            {
                int? year = Int32.Parse(Request.Query["year"]);
                string? title = Request.Query["title"];
                string selectQuery;

                if (year < 1850 ||
                    String.IsNullOrEmpty(title))
                    return BadRequest("Please, to update a movie, you need to fill the mandatory fields: year (higher than 1850) and title.");

                selectQuery = string.Format("SELECT * FROM {0} WHERE year={1} AND title=\"{2}\"", _moviesDAO.dbName, year, title);

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
        [Route("CreateMovie")]
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

                IActionResult response = StatusCode(201);

                if (httpReturnStatus == 409) response = Conflict($"Movie already exists in database.");
                if (httpReturnStatus == 500) response = StatusCode(500, "Internal server error.");

                return response;
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPut]
        [Route("UpdateMovie")]
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
                if (httpReturnStatus == 500) response = StatusCode(500, "Internal server error.");

                return response;
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPatch]
        [Route("UpdatePartialMovie")]
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
                if (httpReturnStatus == 500) return StatusCode(500, "Internal server error.");

                string selectQuery = string.Format("SELECT * FROM {0} WHERE year={1} AND title=\"{2}\"", _moviesDAO.dbName, movie.year, movie.title);
                List<MoviesModel> movieUpdated = _moviesDAO.Read(selectQuery);
                return Ok(movieUpdated[0]);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete]
        [Route("DeleteMovie")]
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
                if (httpReturnStatus == 500) response = StatusCode(500, "Internal server error.");
                return response;
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
