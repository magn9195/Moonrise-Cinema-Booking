using CinemaAPI.BusinessLogic;
using CinemaAPI.BusinessLogic.Interfaces;
using CinemaAPI.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CinemaAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ShowtimesController : ControllerBase
	{
		private readonly ILogger<ShowtimesController> _logger;
		private readonly IShowtimeService _showtimeService;

		public ShowtimesController(ILogger<ShowtimesController> logger, IShowtimeService showtimeService)
		{
			_logger = logger;
			_showtimeService = showtimeService;
		}

		[HttpGet("city/{city}")]
		[ProducesResponseType(typeof(IEnumerable<GetShowtimeDTO>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<IEnumerable<GetShowtimeDTO>>> GetAllShowtimesByCity(string city)
		{
			try
			{
				var showtimes = await _showtimeService.GetAllShowtimesByCityAsync(city);
				return Ok(showtimes);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving all showtimes");
				return StatusCode(500, ex.Message);
			}
		}

		[HttpGet("auditorium/{auditoriumID}")]
		[ProducesResponseType(typeof(IEnumerable<GetShowtimeDTO>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<IEnumerable<GetShowtimeDTO>>> GetShowtimesByAuditoriumAndDate(int auditoriumID, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
		{
			try
			{
				var showtimes = await _showtimeService.GetShowtimesByAuditoriumDateAsync(auditoriumID, startDate, endDate);
				return Ok(showtimes);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error retrieving showtimes for auditorium ID {auditoriumID} between {startDate} and {endDate}");
				return StatusCode(500, ex.Message);
			}
		}

		[HttpGet("{showtimeID}")]
		[ProducesResponseType(typeof(GetShowtimeDTO), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<GetShowtimeDTO>> GetShowtimeById(int showtimeID)
		{
			try
			{
				var showtime = await _showtimeService.GetShowtimeByIDAsync(showtimeID);
				if (showtime == null)
				{
					return NotFound();
				}
				return Ok(showtime);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error retrieving showtime with ID {showtimeID}");
				return StatusCode(500, ex.Message);
			}
		}

		[HttpGet("movie/{movieID}")]
		[ProducesResponseType(typeof(IEnumerable<GetShowtimeDTO>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<IEnumerable<GetShowtimeDTO>>> GetMovieShowtimes(int movieID, string? city)
		{
			try
			{
				var showtimes = await _showtimeService.GetMovieShowtimesAsync(movieID, city);
				return Ok(showtimes);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error retrieving showtimes for movie ID {movieID}");
				return StatusCode(500, ex.Message);
			}
		}

		[HttpGet("{showtimeID}/seats/{seatID}")]
		public async Task<ActionResult<GetShowtimeSeatDTO>> GetShowtimeSeatById(int showtimeID, int seatID)
		{
			try
			{
				var showtimeSeat = await _showtimeService.GetShowtimeSeatByIDAsync(showtimeID, seatID);
				if (showtimeSeat == null)
				{
					return NotFound();
				}
				return Ok(showtimeSeat);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error retrieving seat ID {seatID} for showtime ID {showtimeID}");
				return StatusCode(500, ex.Message);
			}
		}

		[HttpPost]
		[ProducesResponseType(typeof(GetShowtimeDTO), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<int>> CreateShowtime([FromBody] PostShowtimeDTO createShowtimeDTO)
		{
			try
			{
				var newShowtimeID = await _showtimeService.CreateShowtimeAsync(createShowtimeDTO);
				return CreatedAtAction(nameof(GetShowtimeById), new { showtimeID = newShowtimeID }, newShowtimeID);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating new showtime");
				return StatusCode(500, ex.Message);
			}
		}

		[HttpPut("{showtimeID}")]
		[ProducesResponseType(typeof(GetShowtimeDTO), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult> UpdateShowtime(int showtimeID, [FromBody] PutShowtimeDTO updateShowtimeDTO)
		{
			try
			{
				var updatedShowtime = await _showtimeService.UpdateShowtimeAsync(showtimeID, updateShowtimeDTO);
				if (updatedShowtime == null)
				{
					return NotFound();
				}
				return Ok(updatedShowtime);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error updating showtime with ID {showtimeID}");
				return StatusCode(500, ex.Message);
			}
		}

		[HttpDelete("{showtimeID}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult> DeleteShowtime(int showtimeID)
		{
			try
			{
				var deleted = await _showtimeService.DeleteShowtimeAsync(showtimeID);
				if (!deleted)
				{
					return NotFound();
				}
				return NoContent();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error deleting showtime with ID {showtimeID}");
				return StatusCode(500, ex.Message);
			}
		}
	}
}
