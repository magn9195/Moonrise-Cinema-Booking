using System.Text.Json.Serialization;
using CinemaWebService.Models.Enum;

namespace CinemaWebService.Models
{
	public class PostShowtimeSeatCS
	{
		public required int ShowtimeId { get; set; }
		public required int SeatId { get; set; }
	}
}
