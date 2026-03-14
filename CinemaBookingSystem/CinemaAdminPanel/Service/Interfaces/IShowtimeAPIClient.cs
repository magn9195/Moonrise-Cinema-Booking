using CinemaAdminPanel.Models;

namespace CinemaAdminPanel.Service.Interfaces
{
	public interface IShowtimeAPIClient
	{
		Task<IEnumerable<GetShowtimeCS>?> GetAllShowtimesByCityAsync(string city);
		Task<IEnumerable<GetShowtimeCS>?> GetShowtimesByAuditoriumDateAsync(int auditoriumID, DateTime startDate, DateTime endDate);
		Task<IEnumerable<GetShowtimeCS>?> GetMovieShowtimesAsync(int movieID, string city);
		Task<GetShowtimeSeatCS?> GetShowtimeSeatByIdAsync(int showtimeID, int seatID);
		Task<GetShowtimeCS?> GetShowtimeByIdAsync(int showtimeID);
		Task<GetShowtimeCS> CreateShowtimeAsync(PostShowtimeCS showtime);
		Task<GetShowtimeCS> UpdateShowtimeAsync(int showtimeID, PutShowtimeCS showtime);
		Task<bool> DeleteShowtimeAsync(int showtimeID);
	}
}
