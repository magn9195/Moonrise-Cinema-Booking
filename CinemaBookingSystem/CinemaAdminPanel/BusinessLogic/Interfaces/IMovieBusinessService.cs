using CinemaAdminPanel.GUI.DisplayModels;
using CinemaAdminPanel.Models;

namespace CinemaAdminPanel.BusinessLogic.Interfaces
{
	public interface IMovieBusinessService
	{
		Task<List<MovieDM>> GetAllMoviesForDisplayAsync();
		Task<IEnumerable<GetMovieCS>> GetAllMoviesAsync();
		Task<GetMovieCS?> GetMovieByIDAsync(int movieId);
		Task<MovieDM?> CreateMovieAsync(PostMovieCS movieCS);
		Task<MovieDM?> UpdateMovieAsync(int movieId, PostMovieCS movieCS);
		Task<bool> DeleteMovieAsync(int movieId);
	}
}
