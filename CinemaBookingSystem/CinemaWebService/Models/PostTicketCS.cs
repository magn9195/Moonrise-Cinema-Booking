using System.Text.Json.Serialization;
using CinemaWebService.Models;

namespace CinemaWebService.Models
{
	public class PostTicketCS
	{
		public PostCustomerCS? Customer { get; set; }
		public List<PostTicketTypeQuantityCS> TicketTypeQuantities { get; set; } = [];
		public List<PostShowtimeSeatCS> BookedSeats { get; set; } = [];
	}
}
