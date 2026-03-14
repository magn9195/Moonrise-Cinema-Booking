using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using CinemaWebService.Models;
using CinemaWebService.Service.Interfaces;
using Microsoft.Extensions.Options;

namespace CinemaWebService.Service
{
	public class CinemaAPIRestfulClient : ICinemaAPIClient
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<CinemaAPIRestfulClient> _logger;
		private readonly JsonSerializerOptions _jsonOptions;

		public CinemaAPIRestfulClient(HttpClient httpClient, ILogger<CinemaAPIRestfulClient> logger)
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

		// Fetch all cinemas, optionally filtered by city
		public async Task<List<GetCinemaCS>?> GetAllCinemasAsync(string? city)
		{
			try
			{
				var response = await _httpClient.GetFromJsonAsync<List<GetCinemaCS>>($"api/cinemas?city={city}");
				return response;
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, "Error fetching all cinemas from API");
				return null;
			}
		}

		// Fetch all cities
		public async Task<IEnumerable<GetCityZipcodeCS>> GetAllCitiesAsync()
		{
			try
			{
				var response = await _httpClient.GetFromJsonAsync<IEnumerable<GetCityZipcodeCS>>("api/cinemas/cities");
				return response;
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, "Error fetching all cities from API");
				return null;
			}
		}

		// Fetch a specific city by its name
		public async Task<GetCityZipcodeCS?> GetCityByNameAsync(string cityName)
		{
			try
			{
				var response = await _httpClient.GetFromJsonAsync<GetCityZipcodeCS>($"api/cinemas/cities/{cityName}");
				return response;
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, $"Error fetching city '{cityName}' from API");
				return null;
			}
		}

		// Fetch a specific cinema by its ID
		public async Task<GetCinemaCS?> GetCinemaByIDAsync(int cinemaID)
		{
			try
			{
				var response = await _httpClient.GetFromJsonAsync<GetCinemaCS>($"api/cinemas/{cinemaID}");
				return response;
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, $"Error fetching cinema with ID {cinemaID} from API");
				return null;
			}
		}

		// Fetch a specific seat by its ID
		public async Task<GetSeatCS?> GetSeatByIDAsync(int seatID)
		{
			try
			{
				var response = await _httpClient.GetAsync($"api/cinemas/seats/{seatID}");
				response.EnsureSuccessStatusCode();

				var stream = await response.Content.ReadAsStreamAsync();
				return await JsonSerializer.DeserializeAsync<GetSeatCS>(stream, _jsonOptions);
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, $"Error fetching seat with ID {seatID} from API");
				return null;
			}
		}
	}
}
