using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaAdminPanel.GUI.DisplayModels;
using CinemaAdminPanel.Models;

namespace CinemaAdminPanel.BusinessLogic.Interfaces
{
	public interface IShowtimeBusinessService
	{
		Task<IEnumerable<ShowtimeDM>> GetShowtimesByAuditoriumDateAsync(int auditoriumID, DateTime startDate, DateTime endDate);
		Task<GetShowtimeCS?> GetShowtimeByIDAsync(int showtimeID);
		Task<GetShowtimeCS?> CreateShowtimeAsync(PostShowtimeCS showtime);
		Task<bool> DeleteShowtimeAsync(int showtimeID);
		Task<GetShowtimeCS?> UpdateShowtimeAsync(int showtimeID, PutShowtimeCS showtime);
	}
}
