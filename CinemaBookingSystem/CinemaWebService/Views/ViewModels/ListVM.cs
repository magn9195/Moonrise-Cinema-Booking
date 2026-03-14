using CinemaWebService.Models;
using CinemaWebService.Models.Enum;

namespace CinemaWebService.Views.ViewModels
{
	public class ListVM
	{
		public required Dictionary<GetMovieCS, List<string>> Movies { get; set; }
		public required string CityName { get; set; }
		public required List<string> Genres { get; set; }
		public required List<string> Languages { get; set; }
	}
}
