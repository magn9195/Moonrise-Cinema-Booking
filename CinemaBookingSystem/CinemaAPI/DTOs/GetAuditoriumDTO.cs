namespace CinemaAPI.DTOs
{
	public class GetAuditoriumDTO
	{
		public int AuditoriumID { get; set; }
		public required string Name { get; set; }
		public required int RowNum { get; set; }
		public required int SeatsPerRow { get; set; }
		public int CinemaID { get; set; }
	}
}
