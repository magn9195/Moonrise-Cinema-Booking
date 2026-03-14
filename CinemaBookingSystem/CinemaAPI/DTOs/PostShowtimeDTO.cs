using CinemaAPI.Core.Models;

namespace CinemaAPI.DTOs
{
	public class PostShowtimeDTO
	{
		public required string ShowType { get; set; }
		public required DateTime StartTime { get; set; }
		public required int MovieID { get; set; }
		public required int AuditoriumID { get; set; }
	}
}
