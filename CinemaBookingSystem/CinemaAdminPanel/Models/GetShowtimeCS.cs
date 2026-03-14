namespace CinemaAdminPanel.Models
{
	public class GetShowtimeCS
	{
		public required int ShowtimeID { get; set; }
		public required string ShowType { get; set; }
		public required DateTime StartTime { get; set; }
		public required GetMovieCS Movie { get; set; }
		public required GetAuditoriumCS Auditorium { get; set; }
		public required List<GetShowtimeSeatCS> SeatAvailability { get; set; }
	}
}
