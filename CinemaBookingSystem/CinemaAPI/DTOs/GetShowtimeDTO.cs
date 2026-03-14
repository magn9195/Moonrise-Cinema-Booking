using CinemaAPI.Core.Models;

namespace CinemaAPI.DTOs
{
	public class GetShowtimeDTO
	{
		public int ShowtimeID { get; set; }
		public required string ShowType { get; set; }
		public required DateTime StartTime { get; set; }
		public GetMovieDTO? Movie { get; set; }
		public Auditorium? Auditorium { get; set; } = null;
		public required List<ShowtimeSeat> SeatAvailability { get; set; }
	}
}
