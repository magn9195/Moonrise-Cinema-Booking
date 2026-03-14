using CinemaWebService.Models;
using CinemaWebService.Models.Enum;
using CinemaWebService.Views.ViewModels;

namespace CinemaWebService.BusinessLogic.BusinessHelper
{
	public static class VMCombiner
	{
		public static ListVM CombineList(Dictionary<GetMovieCS, List<string>> movies, string cityName, List<string> genres, List<string> languages)
		{
			return new ListVM
			{
				Movies = movies,
				CityName = cityName,
				Genres = genres,
				Languages = languages
			};
		}

		public static DetailsVM CombineDetails(GetMovieCS movie, List<string> imageURLs, Dictionary<GetCinemaCS, IEnumerable<GetShowtimeCS>> showtimes, string cityName)
		{
			return new DetailsVM
			{
				Movie = movie,
				ImageURLs = imageURLs,
				Showtimes = showtimes,
				CityName = cityName
			};
		}

		public static ShowtimeDetailsVM CombineShowtimeDetails(GetMovieCS movie, List<string> imageDataUrls, GetShowtimeCS showtime, GetCinemaCS cinema)
		{
			return new ShowtimeDetailsVM
			{
				Movie = movie,
				ImageURLs = imageDataUrls,
				Showtime = showtime,
				Cinema = cinema
			};
		}

		public static TicketDetailsVM CombineTicketDetails(List<GetSeatCS> bookedSeats, Dictionary<PostTicketTypeQuantityCS, float> pricePerTicketTypeDict, float totalPrice)
		{
			TicketDetailsVM ticketDetailsVM = new TicketDetailsVM
			{
				BookedSeats = bookedSeats,
				PricePerTicketType = pricePerTicketTypeDict,
				TotalPrice = totalPrice
			};
			return ticketDetailsVM;
		}

		public static OrderVM CombineOrder(ShowtimeDetailsVM showtimeDetailsVM, TicketDetailsVM ticketDetailsVM)
		{
			return new OrderVM
			{
				ShowtimeDetails = showtimeDetailsVM,
				TicketDetails = ticketDetailsVM
			};
		}
	}
}
