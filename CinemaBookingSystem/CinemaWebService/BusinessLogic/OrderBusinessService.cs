using CinemaWebService.BusinessLogic.BusinessHelper;
using CinemaWebService.BusinessLogic.Interfaces;
using CinemaWebService.Models;
using CinemaWebService.Service.Interfaces;
using CinemaWebService.Views.ViewModels;

namespace CinemaWebService.BusinessLogic
{
	public class OrderBusinessService : IOrderBusinessService
	{
		private readonly ITicketAPIClient _ticketApiClient;
		private readonly ICinemaAPIClient _cinemaApiClient;
		private readonly IShowtimeAPIClient _showtimeApiClient;
		private readonly IMovieAPIClient _movieApiClient;
		private readonly ILogger<OrderBusinessService> _logger;

		public OrderBusinessService(ITicketAPIClient ticketApiClient, ICinemaAPIClient cinemaApiClient, IShowtimeAPIClient showtimeApiClient, IMovieAPIClient movieApiClient, ILogger<OrderBusinessService> logger)
		{
			_ticketApiClient = ticketApiClient;
			_cinemaApiClient = cinemaApiClient;
			_showtimeApiClient = showtimeApiClient;
			_movieApiClient = movieApiClient;
			_logger = logger;
		}

		public async Task<OrderVM> BuildOrderViewModelAsync(PostTicketCS partialTicket, int movieID, int showtimeID)
		{
			// Fetch seats directly
			var bookedSeats = await FetchSeatsAsync(partialTicket.BookedSeats);

			// Calculate prices directly
			var pricePerTicketTypeDict = await CalculatePricesAsync(partialTicket.TicketTypeQuantities);
			var totalPrice = CalculateTotalPrice(pricePerTicketTypeDict);

			// Fetch showtime details directly
			var showtime = await _showtimeApiClient.GetShowtimeByIdAsync(showtimeID) ?? throw new Exception("Could not find showtime");
			var movie = await _movieApiClient.GetMovieByIdAsync(movieID) ?? throw new Exception("Could not find movie");
			var cinema = await _cinemaApiClient.GetCinemaByIDAsync(showtime.Auditorium.CinemaID) ?? throw new Exception("Could not find cinema"); ;

			List<string> imageUrls = movie.ImageUrls;

			var showtimeDetailsVM = VMCombiner.CombineShowtimeDetails(movie, imageUrls, showtime, cinema);
			var ticketDetailsVM = VMCombiner.CombineTicketDetails(bookedSeats, pricePerTicketTypeDict, totalPrice);

			return VMCombiner.CombineOrder(showtimeDetailsVM, ticketDetailsVM);
		}

		private async Task<List<GetSeatCS>> FetchSeatsAsync(List<PostShowtimeSeatCS> seats)
		{
			List<GetSeatCS> bookedSeats = [];
			foreach (var seat in seats)
			{
				var current = await _cinemaApiClient.GetSeatByIDAsync(seat.SeatId);
				if (current != null) bookedSeats.Add(current);
			}
			return bookedSeats;
		}

		private async Task<Dictionary<PostTicketTypeQuantityCS, float>> CalculatePricesAsync(List<PostTicketTypeQuantityCS> ticketTypeQuantities)
		{
			Dictionary<PostTicketTypeQuantityCS, float> pricePerTicketTypeDict = [];
			foreach (var ticketType in ticketTypeQuantities)
			{
				int quantity = ticketType.Quantity;
				float currentPrice = await _ticketApiClient.FetchCurrentPrice(ticketType.TicketType);
				pricePerTicketTypeDict[ticketType] = currentPrice;
			}
			return pricePerTicketTypeDict;
		}

		private float CalculateTotalPrice(Dictionary<PostTicketTypeQuantityCS, float> pricePerTicketTypeDict)
		{
			return pricePerTicketTypeDict.Sum(entry => entry.Value * entry.Key.Quantity);
		}
	}
}
