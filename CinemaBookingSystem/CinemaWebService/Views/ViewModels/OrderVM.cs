using CinemaWebService.Models;

namespace CinemaWebService.Views.ViewModels
{
	public class OrderVM
	{
		public required ShowtimeDetailsVM ShowtimeDetails { get; set; }
		public required TicketDetailsVM TicketDetails { get; set; }
	}
}
