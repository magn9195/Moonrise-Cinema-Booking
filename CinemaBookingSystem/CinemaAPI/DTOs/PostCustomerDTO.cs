namespace CinemaAPI.DTOs
{
	public class GetCustomerDTO
	{
		public int CustomerID { get; set; }
		public required string Name { get; set; }
		public required string Email { get; set; }
		public required string PhoneNo { get; set; }
		public DateTime CreationDate { get; set; }
	}
}
