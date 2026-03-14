using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaAPI.Core.Models.Enum;

namespace CinemaAPI.Core.Models
{
	public class Ticket
	{
		public int TicketID { get; set; }
		public required Customer Customer { get; set; }
		public float Price { get; set; }
		public List<TicketTypeQuantity> TicketTypeQuantities { get; set; } = new();
		public List<ShowtimeSeat> BookedSeats { get; set; } = new();
		public required DateTime PurchaseDate { get; set; }
		public required DateTime ExpireDate { get; set; }
	}
}
