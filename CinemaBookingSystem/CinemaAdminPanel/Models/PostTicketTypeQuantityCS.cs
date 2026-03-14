using CinemaAdminPanel.Models.Enum;

namespace CinemaAdminPanel.Models
{
	public class PostTicketTypeQuantityCS
	{
		public required TicketTypeEnum TicketType { get; set; }
		public required int Quantity { get; set; }
	}
}
