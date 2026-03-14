using CinemaWebService.Models;

namespace CinemaWebService.Views.ViewModels
{
	public class DetailsVM
	{
		public required GetMovieCS Movie { get; set; }
		public required List<string> ImageURLs { get; set; } = [];
		public required Dictionary<GetCinemaCS, IEnumerable<GetShowtimeCS>> Showtimes { get; set; }
		public required string CityName { get; set; }
	}
}
