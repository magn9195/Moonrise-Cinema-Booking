using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaAdminPanel.Models.Enum;

namespace CinemaAdminPanel.Models
{
	public class GetShowtimeSeatCS
	{
		public required GetSeatCS Seat { get; set; }
		public required SeatStatusEnum Status { get; set; }
		public GetTicketCS? Ticket { get; set; }
	}
}
