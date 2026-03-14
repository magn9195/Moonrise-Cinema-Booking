using CinemaWebService.BusinessLogic.Interfaces;
using CinemaWebService.Models;
using CinemaWebService.Models.Enum;
using CinemaWebService.Service.Interfaces;
using CinemaWebService.Views.ViewModels;

namespace CinemaWebService.BusinessLogic
{
	public class TicketBusinessService : ITicketApiClient
	{
		private readonly ITicketAPIClient _ticketApiClient;
		private readonly ILogger<TicketBusinessService> _logger;

		public TicketBusinessService(ITicketAPIClient ticketApiClient, ILogger<TicketBusinessService> logger)
		{
			_ticketApiClient = ticketApiClient;
			_logger = logger;
		}

		public async Task<int> CheckReservations()
		{
			return await _ticketApiClient.CheckReservations();
		}

		public async Task<bool> ReserveSeatsAsync(List<int> selectedSeats, int showtimeID)
		{
			List<PostShowtimeSeatCS> reservedSeats = new List<PostShowtimeSeatCS>();
			foreach (var id in selectedSeats)
			{
				reservedSeats.Add(new PostShowtimeSeatCS
				{
					ShowtimeId = showtimeID,
					SeatId = id,
				});
			}
			return await _ticketApiClient.ReserveSeatsAsync(reservedSeats);
		}

		public async Task<bool> CreateTicketAsync(PostTicketCS ticket)
		{
			return await _ticketApiClient.CreateTicketAsync(ticket);
		}

		public List<PostTicketTypeQuantityCS> GenerateTicketTypeQuantities(List<string> TicketTypes)
		{
			List<PostTicketTypeQuantityCS> ticketTypeQuantities = new List<PostTicketTypeQuantityCS>();
			foreach (string ticketType in TicketTypes)
			{
				TicketTypeEnum parsedType = Enum.Parse<TicketTypeEnum>(ticketType);
				// Check if the ticket type already exists in the list
				var existing = ticketTypeQuantities.FirstOrDefault(ttq => ttq.TicketType == parsedType);
				if (existing != null)
				{
					// Increment quantity if it exists
					existing.Quantity++;
				}
				else
				{
					// Add new entry if it doesn't exist
					PostTicketTypeQuantityCS ttq = new PostTicketTypeQuantityCS
					{
						TicketType = parsedType,
						Quantity = 1
					};
					ticketTypeQuantities.Add(ttq);
				}
			}
			return ticketTypeQuantities;
		}

		public List<PostShowtimeSeatCS> GeneratePostShowtimeSeats(List<int> seatIDs, int showtimeID)
		{
			List<PostShowtimeSeatCS> showtimeSeats = new List<PostShowtimeSeatCS>();
			foreach (var id in seatIDs)
			{
				showtimeSeats.Add(new PostShowtimeSeatCS
				{
					ShowtimeId = showtimeID,
					SeatId = id,
				});
			}
			return showtimeSeats;
		}

		public async Task<ResultSeatReservationCS> ProcessSeatReservationAsync(List<int> seatIDs, int showtimeID, List<string> ticketTypes)
		{
			var ticketTypeQuantities = GenerateTicketTypeQuantities(ticketTypes);
			var bookedSeats = GeneratePostShowtimeSeats(seatIDs, showtimeID);
			var reservationSuccess = await ReserveSeatsAsync(seatIDs, showtimeID);

			bool valid = reservationSuccess && ticketTypeQuantities.Any() && bookedSeats.Any();

			return new ResultSeatReservationCS
			{
				Success = valid,
				BookedSeats = bookedSeats,
				PartialTicket = valid ? new PostTicketCS {
					TicketTypeQuantities = ticketTypeQuantities,
					BookedSeats = bookedSeats
				} : null
			};
		}
	}
}