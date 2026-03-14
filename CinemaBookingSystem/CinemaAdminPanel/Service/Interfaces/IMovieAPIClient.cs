using CinemaAdminPanel.Models;

namespace CinemaAdminPanel.Service.Interfaces
{
	public interface IMovieAPIClient
	{
		Task<IEnumerable<GetMovieCS>?> GetAllMoviesAsync();
		Task<IEnumerable<GetMovieCS>?> GetAllMoviesByCityAsync(string city);
		Task<GetMovieCS?> GetMovieByIdAsync(int movieID);
		Task<GetMovieCS> CreateMovieAsync(PostMovieCS movie);
		Task<GetMovieCS> UpdateMovieAsync(int movieID, PostMovieCS movie);
		Task<bool> DeleteMovieAsync(int movieID);
		Task<byte[]?> DownloadImageAsync(string imageUrl);
	}
}
