using CinemaAPI.Core.Models;

namespace CinemaAPI.DTOs
{
	public class GetTicketDTO
	{
		public int TicketID { get; set; }
		public required GetCustomerDTO Customer { get; set; }
		public required float Price { get; set; }
		public required DateTime PurchaseDate { get; set; }
		public required DateTime ExpireDate { get; set; }
	}
}
