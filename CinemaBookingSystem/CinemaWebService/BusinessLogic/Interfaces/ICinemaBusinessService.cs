using CinemaWebService.Models;
using CinemaWebService.Views.ViewModels;

namespace CinemaWebService.BusinessLogic.Interfaces
{
	public interface ICinemaBusinessService
	{
		Task<List<GetCinemaCS>?> GetAllCinemasAsync(string? city);
		Task<GetCinemaCS?> GetCinemaByIDAsync(int cinemaID);
		Task<List<GetSeatCS>> GetSeatsFromPostShowtimeSeatsAsync(List<PostShowtimeSeatCS> seats);
		Task<IEnumerable<GetCityZipcodeCS>> GetAllCitiesAsync();
	}
}
