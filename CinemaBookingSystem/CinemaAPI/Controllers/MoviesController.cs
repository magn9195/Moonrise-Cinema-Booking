using CinemaAPI.BusinessLogic;
using CinemaAPI.BusinessLogic.Interfaces;
using CinemaAPI.Core.Models;
using CinemaAPI.Core.Models.Enum;
using CinemaAPI.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CinemaAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class MoviesController : ControllerBase
	{
		private readonly ILogger<CinemasController> _logger;
		private readonly IMovieService _movieService;

		public MoviesController(ILogger<CinemasController> logger, IMovieService movieService)
		{
			_logger = logger;
			_movieService = movieService;
		}

		[HttpGet]
		[ProducesResponseType(typeof(IEnumerable<GetMovieDTO>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<IEnumerable<GetMovieDTO>>> GetAllMovies(string? city, string? genre, LanguageEnum? language, string? age)
		{
			try
			{
				var movies = await _movieService.GetAllMoviesAsync(city, genre, language, age);
				return Ok(movies);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving all movies");
				return StatusCode(500, ex.Message);
			}
		}

		[HttpGet("{movieID}")]
		[ProducesResponseType(typeof(GetShowtimeDTO), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<GetMovieDTO>> GetMovieByID(int movieID)
		{
			try
			{
				var movie = await _movieService.GetMovieByIDAsync(movieID);
				if (movie == null)
				{
					return NotFound();
				}
				return Ok(movie);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error retrieving movie with ID {movieID}");
				return StatusCode(500, ex.Message);
			}
		}

		[HttpGet("{movieID}/images/{imageIndex}")]
		[ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetMovieImage(int movieID, int imageIndex)
		{
			try
			{
				var imageData = await _movieService.GetMovieImageByIndexAsync(movieID, imageIndex);

				if (imageData == null || imageData.Length == 0)
				{
					return NotFound($"Image {imageIndex} not found for movie {movieID}");
				}

				return File(imageData, "image/jpeg", enableRangeProcessing: true);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error retrieving image {imageIndex} for movie {movieID}");
				return StatusCode(500, ex.Message);
			}
		}

		[HttpPost]
		[ProducesResponseType(typeof(GetMovieDTO), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<GetMovieDTO>> CreateMovie([FromBody] PostMovieDTO movie)
		{
			try
			{
				var createdMovie = await _movieService.CreateMovieAsync(movie);
				return CreatedAtAction(nameof(GetMovieByID), new { movieID = createdMovie.MovieID }, createdMovie);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating new movie");
				return StatusCode(500, ex.Message);
			}
		}

		[HttpDelete("{movieID}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> DeleteMovie(int movieID)
		{
			try
			{
				bool isDeleted = await _movieService.DeleteMovieAsync(movieID);
				if (!isDeleted)
				{
					return NotFound();
				}
				return NoContent();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error deleting movie with ID {movieID}");
				return StatusCode(500, ex.Message);
			}
		}

		[HttpPut("{movieID}")]
		[ProducesResponseType(typeof(GetMovieDTO), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> UpdateMovie(int movieID, [FromBody] PostMovieDTO movie)
		{
			try
			{
				var updatedMovie = await _movieService.UpdateMovieAsync(movieID, movie);
				if (updatedMovie == null)
				{
					return NotFound();
				}
				return Ok(updatedMovie);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error updating movie with ID {movieID}");
				return StatusCode(500, ex.Message);
			}
		}

		[HttpGet("genres/{city}")]
		[ProducesResponseType(typeof(IEnumerable<Genre>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<IEnumerable<string>>> GetGenresFromCityAsync(string city)
		{
			try
			{
				var genres = await _movieService.GetGenresFromCityAsync(city);
				return Ok(genres);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving all genres");
				return StatusCode(500, ex.Message);
			}
		}

		[HttpGet("languages/{city}")]
		[ProducesResponseType(typeof(IEnumerable<LanguageEnum>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<IEnumerable<LanguageEnum>>> GetMovieLanguagesFromCityAsync(string city)
		{
			try
			{
				var languages = await _movieService.GetMovieLanguagesFromCityAsync(city);
				return Ok(languages);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving all languages");
				return StatusCode(500, ex.Message);
			}
		}
	}
}
