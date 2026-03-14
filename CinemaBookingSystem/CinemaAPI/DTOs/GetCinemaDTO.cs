using CinemaAPI.Core.Models;

namespace CinemaAPI.DTOs
{
	public class GetCinemaDTO
	{
		public int CinemaID { get; set; }
		public required string Name { get; set; }
		public required GetAddressDTO Address { get; set; }
		public required List<GetAuditoriumDTO> Auditoriums { get; set; } = [];
	}
}
