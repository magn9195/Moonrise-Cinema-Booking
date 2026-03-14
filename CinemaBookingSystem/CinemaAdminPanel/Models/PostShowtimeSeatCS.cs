using System.Text.Json.Serialization;
using CinemaAdminPanel.Models.Enum;

namespace CinemaAdminPanel.Models
{
	public class PostShowtimeSeatCS
	{
		public required int ShowtimeId { get; set; }
		public required int SeatId { get; set; }
	}
}
