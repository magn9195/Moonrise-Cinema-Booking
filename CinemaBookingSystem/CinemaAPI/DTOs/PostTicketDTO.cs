using CinemaAPI.Core.Models;
using CinemaAPI.Core.Models.Enum;
using System.Text.Json.Serialization;

namespace CinemaAPI.DTOs
{
	public class PostTicketDTO
	{
		public required PostCustomerDTO Customer { get; set; }
		public List<PostTicketTypeQuantityDTO> TicketTypeQuantities { get; set; } = new();
		public List<PostShowtimeSeatDTO> BookedSeats { get; set; } = new();
	}
}
