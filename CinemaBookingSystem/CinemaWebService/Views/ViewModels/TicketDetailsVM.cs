using CinemaWebService.Models;

namespace CinemaWebService.Views.ViewModels
{
	public class TicketDetailsVM
	{
		public required List<GetSeatCS> BookedSeats { get; set; }
		public required Dictionary<PostTicketTypeQuantityCS, float> PricePerTicketType { get; set; }
		public required float TotalPrice { get; set; }
	}
}
