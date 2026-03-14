using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.Json;
using CinemaWebService.BusinessLogic;
using CinemaWebService.BusinessLogic.Interfaces;
using CinemaWebService.Models;
using CinemaWebService.Models.Enum;
using CinemaWebService.Service.SignalHub;
using CinemaWebService.Views.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CinemaWebService.Controllers
{
	public class CinemaController : Controller
	{
		private readonly ILogger<CinemaController> _logger;
		private readonly IMovieBusinessService _movieBusinessService;
		private readonly IShowtimeBusinessService _showtimeBusinessService;
		private readonly ITicketApiClient _ticketBusinessService;
		private readonly IOrderBusinessService _orderBusinessService;
		private readonly ICinemaBusinessService _cinemaBusinessService;
		private readonly IHubContext<SeatBookingHub> _hubContext;

		public CinemaController(ILogger<CinemaController> logger, IMovieBusinessService movieBusinessService, 
			IShowtimeBusinessService showtimeBusinessService, ITicketApiClient ticketBusinessService, 
			IOrderBusinessService orderBusinessService, ICinemaBusinessService cinemaBusinessService, IHubContext<SeatBookingHub> hubContext)
		{
			_logger = logger;
			_movieBusinessService = movieBusinessService;
			_showtimeBusinessService = showtimeBusinessService;
			_ticketBusinessService = ticketBusinessService;
			_orderBusinessService = orderBusinessService;
			_cinemaBusinessService = cinemaBusinessService;
			_hubContext = hubContext;
		}

		// Show Cities Page
		public async Task<IActionResult> Index()
		{
			var cities = await _cinemaBusinessService.GetAllCitiesAsync();
			return View(cities);
		}

		// Show All Movies showing in City
		[HttpGet("MovieList/city/{city}")]
		public async Task<IActionResult> MovieList(string city, string? genre, string? language, string? age)
		{
			Console.WriteLine(city);
			var listVM = await _movieBusinessService.GetListMoviesAsync(city, genre, language, age);
			TempData["City"] = city;
			return View(listVM);
		}

		// Show Movie Details Page
		public async Task<IActionResult> Movies(int movieID, string city)
		{
			var detailsVM = await _movieBusinessService.GetMovieDetailsByIdAsync(movieID, city);
			if (detailsVM == null) return RedirectToAction("Index");

			TempData.Keep("City");
			return View(detailsVM);
		}

		// Show Seat Selection Page
		public async Task<IActionResult> SeatPage(int movieID, int showtimeID)
		{
			int checkedReservations = await _ticketBusinessService.CheckReservations();
			var seatVM = await _showtimeBusinessService.GetShowtimeDetailsAsync(movieID, showtimeID);
			if (seatVM == null || seatVM.Showtime == null) return RedirectToAction("Index");

			TempData.Keep("City");
			return View(seatVM);
		}

		// Process Seat Selection
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> SeatPage(int showtimeID, int movieID, List<int> seatID, List<string> ticketTypes)
		{
			if (!ModelState.IsValid) return View();

			var result = await _ticketBusinessService.ProcessSeatReservationAsync(seatID, showtimeID, ticketTypes);

			if (result.Success)
			{
				var reservationId = Guid.NewGuid();
				var expirationTime = DateTime.UtcNow.AddMinutes(15);

				await NotifyClientsOfReservation(result.BookedSeats, showtimeID);

				TempData["PartialTicket"] = JsonSerializer.Serialize(result.PartialTicket);
				TempData["ReservationId"] = reservationId.ToString();
				TempData["ExpirationTimeMs"] = ((DateTimeOffset)expirationTime).ToUnixTimeMilliseconds().ToString();
				TempData.Keep("City");

				return RedirectToAction("MovieOrder", new { id = reservationId, showtimeID, movieID });
			}
			TempData["ErrorMessage"] = "Failed to reserve seats. They may have been taken by another customer.";
			return RedirectToAction("SeatPage", new { movieID, showtimeID });
		}

		// Show Order Page
		public async Task<IActionResult> MovieOrder(Guid id, int showtimeID, int movieID)
		{
			if (TempData["PartialTicket"] == null || TempData["ReservationId"] == null)
			{
				return RedirectToAction("MovieList", new { city = TempData["City"]?.ToString() });
			}

			var storedReservationId = Guid.Parse(TempData["ReservationId"].ToString());
			if (id != storedReservationId)
			{
				return RedirectToAction("MovieList", new { city = TempData["City"]?.ToString() });
			}

			long expirationTimeMs = 0;
			if (TempData["ExpirationTimeMs"] != null)
			{
				expirationTimeMs = long.Parse(TempData["ExpirationTimeMs"].ToString());
				var expirationTime = DateTimeOffset.FromUnixTimeMilliseconds(expirationTimeMs).UtcDateTime;
				if (DateTime.UtcNow >= expirationTime)
				{
					TempData["ErrorMessage"] = "Your reservation has expired. Please select seats again.";
					return RedirectToAction("SeatPage", new { movieID, showtimeID });
				}
				ViewBag.ExpirationTimeMs = expirationTimeMs;
			}

			var partialTicketJson = TempData["PartialTicket"].ToString();
			var partialTicket = JsonSerializer.Deserialize<PostTicketCS>(partialTicketJson);

			TempData.Keep("City");
			ViewBag.PartialTicketJson = partialTicketJson;
			ViewBag.ValidatedReservationId = id;
			ViewBag.ValidatedExpirationMs = expirationTimeMs;

			var orderVM = await _orderBusinessService.BuildOrderViewModelAsync(partialTicket, movieID, showtimeID);
			return View(orderVM);
		}

		// Process Order Submission
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> MovieOrder(Guid id, long expirationTimeMs, string partialTicketJson, PostCustomerCS customer)
		{
			if (!ModelState.IsValid || string.IsNullOrEmpty(partialTicketJson))
			{
				return RedirectToAction("Index");
			}

			if (expirationTimeMs > 0)
			{
				var expirationTime = DateTimeOffset.FromUnixTimeMilliseconds(expirationTimeMs).UtcDateTime;
				if (DateTime.UtcNow >= expirationTime)
				{
					TempData["ErrorMessage"] = "Your reservation has expired. ";
					return RedirectToAction("Index");
				}
			}

			var ticket = JsonSerializer.Deserialize<PostTicketCS>(partialTicketJson);
			ticket.Customer = customer;

			var success = await _ticketBusinessService.CreateTicketAsync(ticket);

			if (success)
			{
				await NotifyClientsOfBooking(ticket.BookedSeats, ticket.BookedSeats.First().ShowtimeId);
			}

			Console.WriteLine("Ticket creation success: " + success);

			string city = TempData["City"]?.ToString();
			return RedirectToAction("MovieList", new { city = city });
		}

		// Get Seat Availability for Showtime (for SignalR)
		[HttpGet]
		public async Task<IActionResult> GetShowtimeSeats(int showtimeID)
		{
			var showtime = await _showtimeBusinessService.GetShowtimeByIDAsync(showtimeID);
			if (showtime?.SeatAvailability == null)
			{
				return NotFound();
			}
			return Json(showtime.SeatAvailability);
		}

		// Notify Clients of Seat Reservation
		private async Task NotifyClientsOfReservation(List<PostShowtimeSeatCS> seats, int showtimeID)
		{
			foreach (var seat in seats)
			{
				await _hubContext.Clients.All.SendAsync("ReceiveSeatReserved", showtimeID, seat.SeatId);
			}
		}

		// Notify Clients of Seat Booking
		private async Task NotifyClientsOfBooking(List<PostShowtimeSeatCS> seats, int showtimeID)
		{
			foreach (var seat in seats)
			{
				await _hubContext.Clients.All.SendAsync("ReceiveSeatBooked", showtimeID, seat.SeatId);
			}
		}
	}
}
