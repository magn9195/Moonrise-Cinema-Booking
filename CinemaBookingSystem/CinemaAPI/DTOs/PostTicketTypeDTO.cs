using CinemaAPI.Core.Models.Enum;

namespace CinemaAPI.DTOs
{
	public class PostTicketTypeDTO
	{
		public required TicketTypeEnum Type { get; set; }
	}
}
