using CinemaAPI.Core.Models;
using CinemaAPI.DTOs;

namespace CinemaAPI.BusinessLogic.Interfaces
{
	public interface ICinemaService
	{
		Task<List<GetCinemaDTO>?> GetAllCinemasAsync(string? city);
		Task<GetCinemaDTO?> GetCinemaByIDAsync(int cinemaID);
		Task<GetSeatDTO?> GetSeatByIDAsync(int seatID);
		Task<IEnumerable<GetCityZipcodeDTO>> GetAllCitiesAsync();
		Task<GetCityZipcodeDTO?> GetCityByNameAsync(string cityName);
	}
}
