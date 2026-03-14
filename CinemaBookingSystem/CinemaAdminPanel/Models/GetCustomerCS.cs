namespace CinemaAdminPanel.Models
{
	public class GetCustomerCS
	{
		public int CustomerID { get; set; }
		public required string Name { get; set; }
		public required string Email { get; set; }
		public required string PhoneNo { get; set; }
		public DateTime CreationDate { get; set; }
	}
}
