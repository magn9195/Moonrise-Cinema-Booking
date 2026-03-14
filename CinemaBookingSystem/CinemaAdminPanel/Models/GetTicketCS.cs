namespace CinemaAdminPanel.Models
{
	public class GetTicketCS
	{
		public int TicketID { get; set; }
		public required GetCustomerCS Customer { get; set; }
		public required float Price { get; set; }
		public required DateTime PurchaseDate { get; set; }
		public required DateTime ExpireDate { get; set; }
	}
}
