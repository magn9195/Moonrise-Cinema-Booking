namespace CinemaAdminPanel.Models
{
	public class GetAddressCS
	{
		public required int HouseNumber { get; set; }
		public required string StreetName { get; set; }
		public required GetCityZipcodeCS CityZipCode { get; set; }
	}
}
