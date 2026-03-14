using CinemaWebService.Models;

namespace CinemaWebService.Views.ViewModels
{
	public class ShowtimeDetailsVM
	{
		public required GetMovieCS Movie { get; set; }
		public required List<string> ImageURLs { get; set; }
		public required GetShowtimeCS Showtime { get; set; }
		public required GetCinemaCS Cinema { get; set; }
	}
}
