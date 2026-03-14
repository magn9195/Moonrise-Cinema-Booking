using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaAPI.Core.Models
{
	public class Customer
	{
		public int CustomerID { get; set; }
		public required string Name { get; set; }
		public required string Email { get; set; }
		public required string PhoneNo { get; set; }
		public required DateTime CreationDate { get; set; }
		public List<Ticket> Tickets { get; set; } = new();
	}
}
