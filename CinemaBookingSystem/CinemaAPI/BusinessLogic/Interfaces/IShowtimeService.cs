using CinemaAPI.Core.Models;
using CinemaAPI.DTOs;

namespace CinemaAPI.BusinessLogic.Interfaces
{
	public interface IShowtimeService
	{
		Task<IEnumerable<GetShowtimeDTO>?> GetAllShowtimesByCityAsync(string city);
		Task<IEnumerable<GetShowtimeDTO>?> GetShowtimesByAuditoriumDateAsync(int auditoriumID, DateTime startDate, DateTime endDate);
		Task<IEnumerable<GetShowtimeDTO>?> GetMovieShowtimesAsync(int movieID, string? city);
		Task<GetShowtimeDTO?> GetShowtimeByIDAsync(int showtimeID);
		Task<GetShowtimeSeatDTO?> GetShowtimeSeatByIDAsync(int showtimeID, int seatID);
		Task<GetShowtimeDTO?> CreateShowtimeAsync(PostShowtimeDTO showtimeDTO);
		Task<bool> DeleteShowtimeAsync(int showtimeID);
		Task<GetShowtimeDTO?> UpdateShowtimeAsync(int showtimeID, PutShowtimeDTO showtimeDTO);
	}
}
