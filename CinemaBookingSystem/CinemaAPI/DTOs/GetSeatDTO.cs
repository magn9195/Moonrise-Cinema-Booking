using CinemaAPI.Core.Models.Enum;
using System.Text.Json.Serialization;

namespace CinemaAPI.DTOs
{
	public class GetSeatDTO
	{
		public int SeatID { get; set; }
		public required int RowNo { get; set; }
		public required int SeatNo { get; set; }
		public required SeatType SeatType { get; set; }
		public int AuditoriumID { get; set; }
	}
}
