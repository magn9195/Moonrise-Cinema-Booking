using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaAPI.Core.Models.Enum;

namespace CinemaAPI.Core.Models
{
	public class TicketTypeQuantity
	{
		public int TicketID { get; set; }
		public TicketTypeEnum TicketType { get; set; }
		public TicketType? TicketTypeInfo { get; set; }
		public Ticket? Ticket { get; set; }
		public required int Quantity { get; set; }
		public required float PricePerTicket { get; set; }
	}
}
