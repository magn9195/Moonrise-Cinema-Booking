using CinemaWebService.Models;

namespace CinemaWebService.Service.Interfaces
{
	public interface IShowtimeAPIClient
	{
		Task<IEnumerable<GetShowtimeCS>?> GetAllShowtimesByCityAsync(string city);
		Task<IEnumerable<GetShowtimeCS>?> GetMovieShowtimesAsync(int movieID, string city);
		Task<GetShowtimeSeatCS?> GetShowtimeSeatByIdAsync(int showtimeID, int seatID);
		Task<GetShowtimeCS?> GetShowtimeByIdAsync(int showtimeID);
	}
}
