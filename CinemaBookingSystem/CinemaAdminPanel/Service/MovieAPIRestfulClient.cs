using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CinemaAdminPanel.GUI.DisplayModels;
using CinemaAdminPanel.Models;
using CinemaAdminPanel.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace CinemaAdminPanel.Service
{
	public class MovieAPIRestfulClient : IMovieAPIClient
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<MovieAPIRestfulClient> _logger;
		private readonly JsonSerializerOptions _jsonOptions;

		public MovieAPIRestfulClient(HttpClient httpClient, ILogger<MovieAPIRestfulClient> logger)
		{
			_httpClient = httpClient;
			_logger = logger;

			// Configure JSON options for enum handling
			_jsonOptions = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
				Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) }
			};
		}

		public async Task<IEnumerable<GetMovieCS>?> GetAllMoviesAsync()
		{
			try
			{
				var response = await _httpClient.GetFromJsonAsync<IEnumerable<GetMovieCS>>("api/movies");
				return response;
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, "Error fetching all movies from API");
				return null;
			}
		}

		public async Task<IEnumerable<GetMovieCS>?> GetAllMoviesByCityAsync(string city)
		{
			try
			{
				var response = await _httpClient.GetFromJsonAsync<IEnumerable<GetMovieCS>>($"api/movies/{city}");
				return response;
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, "Error fetching all movies from API");
				return null;
			}
		}

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

		public async Task<GetMovieCS> CreateMovieAsync(PostMovieCS movie)
		{
			using StringContent jsonContent = new(JsonSerializer.Serialize(movie, _jsonOptions),Encoding.UTF8,"application/json");

			try
			{
				var response = await _httpClient.PostAsync("api/movies", jsonContent);
				response.EnsureSuccessStatusCode();

				var createdMovie = await response.Content.ReadFromJsonAsync<GetMovieCS>(_jsonOptions);
				if(createdMovie == null)
				{
					throw new Exception("Created movie is null");
				}
				return createdMovie;
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, "Error creating new movie via API");
				throw new ApplicationException($"Failed to create movie", ex);
			}
		}

		public async Task<GetMovieCS> UpdateMovieAsync(int movieID, PostMovieCS movie)
		{
			using StringContent jsonContent = new(JsonSerializer.Serialize(movie, _jsonOptions),Encoding.UTF8,"application/json");

			try
			{
				var response = await _httpClient.PutAsync($"api/movies/{movieID}", jsonContent);
				response.EnsureSuccessStatusCode();

				var createdMovie = await response.Content.ReadFromJsonAsync<GetMovieCS>(_jsonOptions);
				if (createdMovie == null)
				{
					throw new Exception("Created movie is null");
				}
				return createdMovie;
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, $"Error updating movie with ID {movieID} via API");
				throw new ApplicationException($"Failed to update movie with ID {movieID}", ex);
			}
		}

		public async Task<bool> DeleteMovieAsync(int movieID)
		{
			try
			{
				var response = await _httpClient.DeleteAsync($"api/movies/{movieID}");
				response.EnsureSuccessStatusCode();
				return true;
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, $"Error deleting movie with ID {movieID} via API");
				return false;
			}
		}

		public async Task<byte[]?> DownloadImageAsync(string imageUrl)
		{
			try
			{
				var response = await _httpClient.GetAsync(imageUrl);
				response.EnsureSuccessStatusCode();
				return await response.Content.ReadAsByteArrayAsync();
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, $"Error downloading image from {imageUrl}");
				return null;
			}
		}
	}
}
