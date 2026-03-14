using CinemaWebService.Models;
using CinemaWebService.Service.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CinemaWebService.Service
{
	public class ShowtimeAPIRestfulClient : IShowtimeAPIClient
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<ShowtimeAPIRestfulClient> _logger;
		private readonly JsonSerializerOptions _jsonOptions;

		public ShowtimeAPIRestfulClient(HttpClient httpClient, ILogger<ShowtimeAPIRestfulClient> logger)
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

		// Fetch all showtimes available in a specific city
		public async Task<IEnumerable<GetShowtimeCS>?> GetAllShowtimesByCityAsync(string city)
		{
			try
			{
				var response = await _httpClient.GetAsync($"api/Showtimes/city/{city}");
				response.EnsureSuccessStatusCode();

				var stream = await response.Content.ReadAsStreamAsync();
				return await JsonSerializer.DeserializeAsync<IEnumerable<GetShowtimeCS>>(stream, _jsonOptions);
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, $"Error fetching all showtimes from City {city} from API");
				return null;
			}
			catch (JsonException ex)
			{
				_logger.LogError(ex, $"Error deserializing showtimes from City {city} from API");
				return null;
			}
		}

		// Fetch showtimes for a specific movie in a specific city
		public async Task<IEnumerable<GetShowtimeCS>?> GetMovieShowtimesAsync(int movieID, string city)
		{
			try
			{
				var response = await _httpClient.GetAsync($"api/Showtimes/movie/{movieID}?city={city}");
				response.EnsureSuccessStatusCode();

				var stream = await response.Content.ReadAsStreamAsync();
				return await JsonSerializer.DeserializeAsync<IEnumerable<GetShowtimeCS>>(stream, _jsonOptions);
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, $"Error fetching showtimes for movie with ID {movieID} from City {city} from API");
				return null;
			}
			catch (JsonException ex)
			{
				_logger.LogError(ex, $"Error deserializing showtimes for movie with ID {movieID} from City {city} from API");
				return null;
			}
		}

		// Fetch specific seat information for a showtime by showtime ID and seat ID
		public async Task<GetShowtimeSeatCS?> GetShowtimeSeatByIdAsync(int showtimeID, int seatID)
		{
			try
			{
				var response = await _httpClient.GetAsync($"api/Showtimes/{showtimeID}/seats/{seatID}");
				response.EnsureSuccessStatusCode();

				var stream = await response.Content.ReadAsStreamAsync();
				var result = await JsonSerializer.DeserializeAsync<GetShowtimeSeatCS>(stream, _jsonOptions);
				
				if (result != null)
				{
					return result;
				}
				return null;
			} catch (Exception ex)
			{
				_logger.LogError(ex, "Error GetShowtimeSeatByIdAsync");
				return null;
			}
		}

		// Fetch a specific showtime by its ID
		public async Task<GetShowtimeCS?> GetShowtimeByIdAsync(int showtimeID)
		{
			try
			{
				var response = await _httpClient.GetAsync($"api/Showtimes/{showtimeID}");
				response.EnsureSuccessStatusCode();

				var stream = await response.Content.ReadAsStreamAsync();
				return await JsonSerializer.DeserializeAsync<GetShowtimeCS>(stream, _jsonOptions);
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, $"Error fetching showtime with ID {showtimeID} from API");
				return null;
			}
			catch (JsonException ex)
			{
				_logger.LogError(ex, $"Error deserializing showtime with ID {showtimeID} from API");
				return null;
			}
		}
	}
}