namespace CinemaWebService.Models
{
	public class GetCityZipcodeCS
	{
		public int CityZipcodeID { get; set; }
		public required string ZipCode { get; set; }
		public required string City { get; set; }
	}
}