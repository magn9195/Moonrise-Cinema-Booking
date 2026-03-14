using CinemaAPI.Core.Models;
using CinemaAPI.Core.Models.Enum;

namespace CinemaAPI.DTOs
{
	public class GetShowtimeSeatDTO
	{
		public int ShowtimeID { get; set; }
		public int SeatID { get; set; }
		public Seat? Seat { get; set; }
		public required SeatStatusEnum Status { get; set; }
		public GetTicketDTO? Ticket { get; set; }
	}
}
