using CinemaAdminPanel.Models;
using CinemaAdminPanel.Service.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CinemaAdminPanel.Service
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

		public async Task<IEnumerable<GetShowtimeCS>?> GetAllShowtimesByCityAsync(string city)
		{
			try
			{
				var response = await _httpClient.GetAsync($"api/Showtimes?city={city}");
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

		public async Task<IEnumerable<GetShowtimeCS>?> GetShowtimesByAuditoriumDateAsync(int auditoriumID, DateTime startDate, DateTime endDate)
		{
			try
			{
				var response = await _httpClient.GetAsync($"api/Showtimes/auditorium/{auditoriumID}?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
				response.EnsureSuccessStatusCode();
				var stream = await response.Content.ReadAsStreamAsync();
				return await JsonSerializer.DeserializeAsync<IEnumerable<GetShowtimeCS>>(stream, _jsonOptions);
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, $"Error fetching showtimes for auditorium with ID {auditoriumID} on Date startDate={startDate:yyyy-MM-dd} & endDate={endDate:yyyy-MM-dd} from API");
				return null;
			}
			catch (JsonException ex)
			{
				_logger.LogError(ex, $"Error deserializing showtimes for auditorium with ID {auditoriumID} on Date startDate={startDate:yyyy-MM-dd} & endDate={endDate:yyyy-MM-dd} from API");
				return null;
			}
		}

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

		public async Task<GetShowtimeCS> CreateShowtimeAsync(PostShowtimeCS showtime)
		{
			using var jsonContent = new StringContent(JsonSerializer.Serialize(showtime, _jsonOptions), System.Text.Encoding.UTF8, "application/json");
			try
			{
				using HttpResponseMessage response = await _httpClient.PostAsync(
					$"api/Showtimes",
					jsonContent);
				response.EnsureSuccessStatusCode();
				var jsonResponse = await response.Content.ReadAsStringAsync();
				var createdShowtime = JsonSerializer.Deserialize<GetShowtimeCS>(jsonResponse, _jsonOptions);
				return createdShowtime!;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating showtime via API");
				throw;
			}
		}

		public async Task<bool> DeleteShowtimeAsync(int showtimeID)
		{
			try
			{
				using HttpResponseMessage response = await _httpClient.DeleteAsync(
					$"api/Showtimes/{showtimeID}");
				response.EnsureSuccessStatusCode();
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error deleting showtime with ID {showtimeID} via API");
				return false;
			}
		}

		public async Task<GetShowtimeCS> UpdateShowtimeAsync(int showtimeID, PutShowtimeCS showtime)
		{
			using var jsonContent = new StringContent(JsonSerializer.Serialize(showtime, _jsonOptions), System.Text.Encoding.UTF8, "application/json");
			try
			{
				using HttpResponseMessage response = await _httpClient.PutAsync(
					$"api/Showtimes/{showtimeID}",
					jsonContent);
				response.EnsureSuccessStatusCode();
				var jsonResponse = await response.Content.ReadAsStringAsync();
				var updatedShowtime = JsonSerializer.Deserialize<GetShowtimeCS>(jsonResponse, _jsonOptions);
				return updatedShowtime!;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error updating showtime with ID {showtimeID} via API");
				throw;
			}
		}
	}
}