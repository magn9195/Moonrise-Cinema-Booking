using CinemaWebService.Models.Enum;

namespace CinemaWebService.Models
{
	public class PostTicketTypeQuantityCS
	{
		public required TicketTypeEnum TicketType { get; set; }
		public required int Quantity { get; set; }
	}
}
