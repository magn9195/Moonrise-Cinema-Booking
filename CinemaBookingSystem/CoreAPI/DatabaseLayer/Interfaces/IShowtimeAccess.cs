using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaAPI.Core.Models;

namespace CinemaAPI.Core.DatabaseLayer.Interfaces
{
	public interface IShowtimeAccess
	{
		Task<IEnumerable<Showtime>?> GetAllShowtimesByCityAsync(string city);
		Task<IEnumerable<Showtime>?> GetShowtimesByAuditoriumDateAsync(int auditoriumID, DateTime startDate, DateTime endDate);
		Task<IEnumerable<Showtime>?> GetMovieShowtimesAsync(int movieID, string? city);
		Task<Showtime?> GetShowtimeByIDAsync(int showtimeID);
		Task<ShowtimeSeat?> GetShowtimeSeatByIDAsync(int showtimeID, int seatID);
		Task<Showtime> CreateShowtimeAsync(Showtime showtime);
		Task<bool> DeleteShowtimeAsync(int showtimeID);
		Task<Showtime> UpdateShowtimeAsync(Showtime showtime);
	}
}
