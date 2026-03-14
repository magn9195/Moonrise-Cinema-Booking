using CinemaAPI.Core.Models;
using CinemaAPI.Core.Models.Enum;

namespace CinemaAPI.DTOs
{
	public class PostTicketTypeQuantityDTO
	{
		public required TicketTypeEnum TicketType { get; set; }
		public required int Quantity { get; set; }
	}
}
