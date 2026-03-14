using System.Text.Json.Serialization;
using CinemaAdminPanel.Models;

namespace CinemaAdminPanel.Models
{
	public class PostTicketCS
	{
		public PostCustomerCS? Customer { get; set; }
		public List<PostTicketTypeQuantityCS> TicketTypeQuantities { get; set; } = new();
		public List<PostShowtimeSeatCS> BookedSeats { get; set; } = new();
	}
}
