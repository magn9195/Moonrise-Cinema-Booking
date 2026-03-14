using CinemaWebService.Models;
using CinemaWebService.Views.ViewModels;

namespace CinemaWebService.BusinessLogic.Interfaces
{
	public interface IShowtimeBusinessService
	{
		Task<ShowtimeDetailsVM?> GetShowtimeDetailsAsync(int movieID, int showtimeID);
		Task<GetShowtimeCS> GetShowtimeByIDAsync(int showtimeID);
	}
}
