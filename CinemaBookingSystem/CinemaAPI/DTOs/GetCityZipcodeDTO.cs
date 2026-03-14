namespace CinemaAPI.DTOs
{
	public class GetCityZipcodeDTO
	{
		public int CityZipcodeID { get; set; }
		public required string ZipCode { get; set; }
		public required string City { get; set; }
	}
}
