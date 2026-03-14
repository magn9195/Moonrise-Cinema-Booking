using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaAdminPanel.BusinessLogic.Interfaces;
using CinemaAdminPanel.GUI.DisplayModels;
using CinemaAdminPanel.Models;
using CinemaAdminPanel.Service.Interfaces;

namespace CinemaAdminPanel.BusinessLogic
{
	public class ShowtimeBusinessService : IShowtimeBusinessService
	{
		private readonly IShowtimeAPIClient _showtimeApiClient;

		public ShowtimeBusinessService(IShowtimeAPIClient showtimeApiClient)
		{
			_showtimeApiClient = showtimeApiClient;
		}

		public async Task<IEnumerable<ShowtimeDM>> GetShowtimesByAuditoriumDateAsync(int auditoriumID, DateTime startDate, DateTime endDate)
		{
			var auditoriumShowtimes = await _showtimeApiClient.GetShowtimesByAuditoriumDateAsync(auditoriumID, startDate, endDate);
			if (auditoriumShowtimes == null) return new List<ShowtimeDM>();
			var displayShowtimes = ConvertToDisplayModels(auditoriumShowtimes);
			if (displayShowtimes == null) return new List<ShowtimeDM>();
			return displayShowtimes;
		}

		public async Task<GetShowtimeCS?> GetShowtimeByIDAsync(int showtimeID)
		{
			var showtime = await _showtimeApiClient.GetShowtimeByIdAsync(showtimeID);
			return showtime;
		}

		public async Task<GetShowtimeCS?> CreateShowtimeAsync(PostShowtimeCS showtime)
		{
			var createdShowtime = await _showtimeApiClient.CreateShowtimeAsync(showtime);
			return createdShowtime;
		}

		public async Task<bool> DeleteShowtimeAsync(int showtimeID)
		{
			var isDeleted = await _showtimeApiClient.DeleteShowtimeAsync(showtimeID);
			return isDeleted;
		}

		public async Task<GetShowtimeCS?> UpdateShowtimeAsync(int showtimeID, PutShowtimeCS showtime)
		{
			var updatedShowtime = await _showtimeApiClient.UpdateShowtimeAsync(showtimeID, showtime);
			return updatedShowtime;
		}

		private List<ShowtimeDM> ConvertToDisplayModels(IEnumerable<GetShowtimeCS> showtimes)
		{
			if (showtimes == null) return new List<ShowtimeDM>();
			List<ShowtimeDM> displayShowtimes = showtimes.Select(s => new ShowtimeDM
			{
				ShowtimeID = s.ShowtimeID,
				Date = s.StartTime.Date,
				StartTime = s.StartTime.ToString("HH:mm"),
				EndTime = s.StartTime.AddMinutes(s.Movie.Duration).ToString("HH:mm"),
				MovieTitle = s.Movie.Title,
				Duration = $"{s.Movie.Duration / 60}h {s.Movie.Duration % 60}m",
				ShowtimeType = s.ShowType,
				GapAfter = "-"
			}).ToList();
			return displayShowtimes;
		}
	}
}
