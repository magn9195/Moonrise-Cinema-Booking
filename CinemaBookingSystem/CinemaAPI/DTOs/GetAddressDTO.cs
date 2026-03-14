using CinemaAPI.Core.Models;

namespace CinemaAPI.DTOs
{
	public class GetAddressDTO
	{
		public required int HouseNumber { get; set; }
		public required string StreetName { get; set; }
		public required GetCityZipcodeDTO CityZipCode { get; set; }
	}
}
