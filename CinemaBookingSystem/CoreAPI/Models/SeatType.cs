using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaAPI.Core.Models.Enum
{
	public class SeatTypeEnum
	{
		public int TicketTypeID { get; set; }
		public required TicketTypeEnum Type { get; set; }
		public required decimal CurrentPrice { get; set; }
		public string? Description { get; set; }
	}
}
