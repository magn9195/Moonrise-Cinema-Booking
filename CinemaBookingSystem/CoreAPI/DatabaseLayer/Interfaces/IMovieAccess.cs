using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaAPI.Core.Models;
using CinemaAPI.Core.Models.Enum;

namespace CinemaAPI.Core.DatabaseLayer.Interfaces
{
	public interface IMovieAccess
	{
		Task<IEnumerable<Movie>?> GetAllMoviesAsync(string? city, string? genre, LanguageEnum? language, string? age);
		Task<Movie?> GetMovieByIDAsync(int movieID);
		Task<Movie> CreateMovieAsync(Movie movie);
		Task<bool> DeleteMovieAsync(int movieID);
		Task<Movie> UpdateMovieAsync(Movie movie);
		Task<List<string>> GetGenresFromCityAsync(string city);
		Task<int> GetMovieImageCountAsync(int movieID);
		Task<byte[]?> GetMovieImageByIndexAsync(int movieID, int imageIndex);
		Task<List<MovieImage>> GetMovieImageIdsAsync(int movieID);
		Task<List<LanguageEnum>> GetMovieLanguagesFromCityAsync(string city);
	}
}
