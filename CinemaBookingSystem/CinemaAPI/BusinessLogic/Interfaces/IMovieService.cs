using CinemaAPI.Core.Models;
using CinemaAPI.Core.Models.Enum;
using CinemaAPI.DTOs;

namespace CinemaAPI.BusinessLogic.Interfaces
{
	public interface IMovieService
	{
		Task<IEnumerable<GetMovieDTO>?> GetAllMoviesAsync(string? city, string? genre, LanguageEnum? language, string? age);
		Task<GetMovieDTO?> GetMovieByIDAsync(int movieID);
		Task<byte[]> GetMovieImageByIndexAsync(int movieID, int imageIndex);
		Task<GetMovieDTO?> CreateMovieAsync(PostMovieDTO movie);
		Task<bool> DeleteMovieAsync(int movieID);
		Task<GetMovieDTO?> UpdateMovieAsync(int movieID, PostMovieDTO movie);
		Task<List<string>> GetGenresFromCityAsync(string city);
		Task<List<LanguageEnum>> GetMovieLanguagesFromCityAsync(string city);
	}
}
