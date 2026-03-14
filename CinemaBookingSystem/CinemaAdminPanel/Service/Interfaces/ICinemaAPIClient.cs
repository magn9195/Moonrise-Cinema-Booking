using CinemaAdminPanel.Models;

namespace CinemaAdminPanel.Service.Interfaces
{
	public interface ICinemaAPIClient
	{
		Task<List<GetCinemaCS>?> GetAllCinemasAsync(string? city);
		Task<GetCinemaCS?> GetCinemaByIDAsync(int cinemaID);
		Task<GetSeatCS?> GetSeatByIDAsync(int seatID);
		Task<IEnumerable<GetCityZipcodeCS>?> GetAllCitiesAsync();
	}
}
