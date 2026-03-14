using CinemaAPI.Core.Models;
using CinemaAPI.Core.Models.Enum;
using System.Text.Json.Serialization;

namespace CinemaAPI.DTOs
{
	public class PostShowtimeSeatDTO
	{
		public required int ShowtimeId { get; set; }
		public required int SeatId { get; set; }
	}
}
