using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaAPI.Core.Models.Enum;

namespace CinemaAPI.Core.Models
{
	public class ShowtimeSeat
	{
		public int ShowtimeID { get; set; }
		public int SeatID { get; set; }
		public Showtime? Showtime { get; set; }
		public Seat? Seat { get; set; }
		public required SeatStatusEnum Status { get; set; }
		public DateTime? ReservedTill { get; set; }
		public int? TicketID { get; set; }
		public Ticket? Ticket { get; set; }
		public byte[]? RowVersion { get; set; }
	}
}
