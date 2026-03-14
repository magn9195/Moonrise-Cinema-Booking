using CinemaWebService.Models;
using CinemaWebService.Models.Enum;
using CinemaWebService.Service.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace CinemaWebService.Service
{
	public class MovieAPIRestfulClient : IMovieAPIClient
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<MovieAPIRestfulClient> _logger;

		public MovieAPIRestfulClient(HttpClient httpClient, ILogger<MovieAPIRestfulClient> logger)
		{
			_httpClient = httpClient;
			_logger = logger;
		}

		// Fetch all movies available in a specific city
		public async Task<IEnumerable<GetMovieCS>?> GetAllMoviesAsync(string city, string? genre, string? language, string? age)
		{
			try
			{

				var query = HttpUtility.ParseQueryString("");

				query.Add("city", city);

				if (genre != null)
				{
					query.Add("genre", genre);
				}
				if (language != null)
				{
					query.Add("language", language);
				}
				if (age != null)
				{
					query.Add("age", age);
				}

				var url = $"api/movies?{query}";

				var response = await _httpClient.GetFromJsonAsync<IEnumerable<GetMovieCS>>(url);
				return response;
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, "Error fetching all movies from API");
				return null;
			}
		}

		// Fetch a specific movie by its ID
		public async Task<GetMovieCS?> GetMovieByIdAsync(int movieID)
		{
			try
			{
				var response = await _httpClient.GetFromJsonAsync<GetMovieCS>($"api/movies/{movieID}");
				return response;
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, $"Error fetching movie with ID {movieID} from API");
				return null;
			}
		}

		public async Task<List<string>> GetGenresFromCityAsync(string city)
		{
			try
			{
				var response = await _httpClient.GetFromJsonAsync<List<string>>($"api/movies/genres/{city}");
				return response;
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, $"Error fetching genre with city: {city} from API");
				return null;
			}
		}

		public async Task<List<string>> GetMovieLanguagesFromCityAsync(string city)
		{
			try
			{
				var response = await _httpClient.GetFromJsonAsync<List<string>>($"api/movies/languages/{city}");
				return response;
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, $"Error fetching languages with city: {city} from API");
				return null;
			}
		}
	}
}
