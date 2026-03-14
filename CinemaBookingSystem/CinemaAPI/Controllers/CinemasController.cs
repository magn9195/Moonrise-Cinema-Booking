using CinemaAPI.BusinessLogic;
using CinemaAPI.BusinessLogic.Interfaces;
using CinemaAPI.Core.Models;
using CinemaAPI.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CinemaAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class CinemasController : ControllerBase
	{
		private readonly ILogger<CinemasController> _logger;
		private readonly ICinemaService _cinemaService;

		public CinemasController(ILogger<CinemasController> logger, ICinemaService cinemaService)
		{
			_logger = logger;
			_cinemaService = cinemaService;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<GetCinemaDTO>>> GetAllCinemas(string? city)
		{
			try
			{
				var cinemas = await _cinemaService.GetAllCinemasAsync(city);
				return Ok(cinemas);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving all cinemas");
				return StatusCode(500, ex.Message);
			}
		}

		[HttpGet("{cinemaID}")]
		public async Task<ActionResult<GetCinemaDTO>> GetCinemaByID(int cinemaID)
		{
			try
			{
				var cinema = await _cinemaService.GetCinemaByIDAsync(cinemaID);
				if (cinema == null)
				{
					return NotFound();
				}
				return Ok(cinema);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error retrieving cinema with ID {cinemaID}");
				return StatusCode(500, ex.Message);
			}
		}

		[HttpGet("seats/{seatID}")]
		public async Task<ActionResult<GetSeatDTO>> GetSeatByID(int seatID)
		{
			try
			{
				var seat = await _cinemaService.GetSeatByIDAsync(seatID);
				if (seat == null)
				{
					return NotFound();
				}
				return Ok(seat);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error retrieving seat with ID {seatID}");
				return StatusCode(500, ex.Message);
			}
		}

		[HttpGet("cities")]
		public async Task<ActionResult<GetCityZipcodeDTO>> GetAllCities()
		{
			try
			{
				var cities = await _cinemaService.GetAllCitiesAsync();
				return Ok(cities);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving all cities");
				return StatusCode(500, ex.Message);
			}
		}

		[HttpGet("cities/{cityName}")]
		public async Task<ActionResult<GetCityZipcodeDTO>> GetCityByName(string cityName)
		{
			try
			{
				var city = await _cinemaService.GetCityByNameAsync(cityName);
				if (city == null)
				{
					return NotFound();
				}
				return Ok(city);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error retrieving city with ID {cityName}");
				return StatusCode(500, ex.Message);
			}
		}
	}
}
