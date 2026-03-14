using CinemaWebService.Models;
using CinemaWebService.Models.Enum;

namespace CinemaWebService.Service.Interfaces
{
	public interface IMovieAPIClient
	{
		Task<IEnumerable<GetMovieCS>?> GetAllMoviesAsync(string city, string? genre, string? language, string? age);
		Task<GetMovieCS?> GetMovieByIdAsync(int movieID);
		Task<List<string>> GetGenresFromCityAsync(string city);
		Task<List<string>> GetMovieLanguagesFromCityAsync(string city);
	}
}
