namespace CinemaWebService.Models
{
	public class GetCinemaCS
	{
		public int CinemaID { get; set; }
		public required string Name { get; set; }
		public required GetAddressCS Address { get; set; }
		public required List<GetAuditoriumCS> Auditoriums { get; set; } = [];
	}
}
