using CinemaWebService.Views.ViewModels;

namespace CinemaWebService.BusinessLogic.Interfaces
{
	public interface IMovieBusinessService
	{
		Task<ListVM?> GetListMoviesAsync(string city, string? genre, string? language, string? age);
		Task<DetailsVM?> GetMovieDetailsByIdAsync(int movieID, string city);
	}
}
