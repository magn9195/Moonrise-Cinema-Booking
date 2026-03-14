using CinemaWebService.BusinessLogic.Interfaces;
using CinemaWebService.Models;
using CinemaWebService.Service.Interfaces;
using CinemaWebService.Views.ViewModels;

namespace CinemaWebService.BusinessLogic
{
	public class CinemaBusinessService : ICinemaBusinessService
	{
		private readonly ICinemaAPIClient _cinemaApiClient;
		private readonly ILogger<CinemaBusinessService> _logger;

		public CinemaBusinessService(ICinemaAPIClient cinemaApiClient, ILogger<CinemaBusinessService> logger)
		{
			_cinemaApiClient = cinemaApiClient;
			_logger = logger;
		}

		public async Task<List<GetCinemaCS>?> GetAllCinemasAsync(string? city)
		{
			var cinemas = await _cinemaApiClient.GetAllCinemasAsync(city);
			if (cinemas == null || !cinemas.Any())
			{
				return null;
			}
			return cinemas;
		}

		public async Task<GetCinemaCS?> GetCinemaByIDAsync(int cinemaID)
		{
			var cinema = await _cinemaApiClient.GetCinemaByIDAsync(cinemaID);
			if (cinema == null)
			{
				return null;
			}
			return cinema;
		}

		public async Task<List<GetSeatCS>> GetSeatsFromPostShowtimeSeatsAsync(List<PostShowtimeSeatCS> seats)
		{
			List<GetSeatCS> bookedSeats = new List<GetSeatCS>();
			foreach (var seat in seats)
			{
				var current = await _cinemaApiClient.GetSeatByIDAsync(seat.SeatId);
				if (current != null) bookedSeats.Add(current);
			}
			return bookedSeats;
		}

		public async Task<IEnumerable<GetCityZipcodeCS>> GetAllCitiesAsync()
		{
			var cities = await _cinemaApiClient.GetAllCitiesAsync();
			return cities;
		}
	}
}
