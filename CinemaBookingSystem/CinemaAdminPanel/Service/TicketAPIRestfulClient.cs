using CinemaAdminPanel.Models;
using CinemaAdminPanel.Models.Enum;
using CinemaAdminPanel.Service.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CinemaAdminPanel.Service
{
	public class TicketAPIRestfulClient : ITicketAPIClient
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<TicketAPIRestfulClient> _logger;
		private readonly JsonSerializerOptions _jsonOptions;

		public TicketAPIRestfulClient(HttpClient httpClient, ILogger<TicketAPIRestfulClient> logger)
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

		public async Task<float> FetchCurrentPrice(TicketTypeEnum ticketType)
		{
			try
			{
				using HttpResponseMessage response = await _httpClient.GetAsync(
					$"api/Tickets/prices/{ticketType}");
				response.EnsureSuccessStatusCode();
				var jsonResponse = await response.Content.ReadAsStringAsync();
				var price = JsonSerializer.Deserialize<float>(jsonResponse, _jsonOptions);
				return price;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error fetching ticket price from API");
				throw;
			}
		}

		public async Task<int> CheckReservations()
		{
			try
			{
				using HttpResponseMessage response = await _httpClient.PostAsync(
					$"api/Tickets/check-reservations",
					null);
				response.EnsureSuccessStatusCode();
				var jsonResponse = await response.Content.ReadAsStringAsync();
				var reservedSeatsCount = JsonSerializer.Deserialize<int>(jsonResponse, _jsonOptions);
				return reservedSeatsCount;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error checking reservations from API");
				throw;
			}
		}

		public async Task<bool> ReserveSeatsAsync(List<PostShowtimeSeatCS> reserveSeats)
		{
			using StringContent jsonContent = new(JsonSerializer.Serialize(reserveSeats, _jsonOptions), Encoding.UTF8, "application/json");
			try
			{
				using HttpResponseMessage response = await _httpClient.PutAsync(
					$"api/Tickets/reserve",
					jsonContent);
				response.EnsureSuccessStatusCode();
				var jsonResponse = await response.Content.ReadAsStringAsync();
				Console.WriteLine($"{jsonResponse}\n");
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error reserving seats via API");
				return false;
			}
		}

		public async Task<bool> CreateTicketAsync(PostTicketCS ticket)
		{
			using StringContent jsonContent = new(JsonSerializer.Serialize(ticket, _jsonOptions), Encoding.UTF8, "application/json");

			try
			{
				using HttpResponseMessage response = await _httpClient.PostAsync(
					$"api/Tickets",
					jsonContent);

				response.EnsureSuccessStatusCode();

				var jsonResponse = await response.Content.ReadAsStringAsync();
				Console.WriteLine($"{jsonResponse}\n");
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error posting tickets to API");
				return false;
			}
		}
	}
}
