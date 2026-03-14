using CinemaAPI.BusinessLogic.Interfaces;
using CinemaAPI.Core.Models;
using CinemaAPI.Core.Models.Enum;
using CinemaAPI.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CinemaAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class TicketsController : ControllerBase
	{
		private readonly ITicketService _ticketService;

		public TicketsController(ITicketService ticketService)
		{
			_ticketService = ticketService;
		}

		[HttpGet("prices/{TicketType}")]
		public async Task<IActionResult> FetchCurrentPrice (TicketTypeEnum TicketType)
		{
			try
			{
				var price = await _ticketService.FetchCurrentPrice(TicketType);
				return Ok(price);
			}
			catch (Exception ex)
			{
				return StatusCode(500, ex.Message);
			}
		}

		[HttpPost("check-reservations")]
		public async Task<IActionResult> CheckReservations()
		{
			try
			{
				var reservedSeats = await _ticketService.CheckReservations();
				return Ok(reservedSeats);
			}
			catch (Exception ex)
			{
				return StatusCode(500, ex.Message);
			}
		}

		[HttpPut("reserve")]
		public async Task<IActionResult> ReserveSeats(List<PostShowtimeSeatDTO> reserveSeatsDTO)
		{
			if (reserveSeatsDTO.Count <= 0)
			{
				return BadRequest("No seats have been selected for reservation.");
			}
			try
			{
				var success = await _ticketService.ReserveSeatsAsync(reserveSeatsDTO);

				if (success)
				{
					return Ok("Seats reserved successfully");
				}
				else
				{
					return Conflict("Unable to reserve seats. They may already be reserved or booked.");
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, ex.Message);
			}
		}

		[HttpPost]
		public async Task<IActionResult> Post(PostTicketDTO ticketDTO)
		{
			if (ticketDTO.BookedSeats.Count <= 0)
			{
				return BadRequest("No seats have been assigned to the ticket.");
			}

			if (ticketDTO.Customer == null)
			{
				return BadRequest("No customer has been assigned to the ticket.");
			}
			try
			{
				var success = await _ticketService.CreateTicketAsync(ticketDTO);

				if (success)
				{
					return Ok("Ticket created successfully");
				}
				else
				{
					return Conflict("Unable to create ticket. Seats may already be booked or reservation may have expired.");
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, ex.Message);
			}
		}
	}
}
