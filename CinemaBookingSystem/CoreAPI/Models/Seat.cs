using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaAPI.Core.Models.Enum;

namespace CinemaAPI.Core.Models
{
	public class Seat
	{
		public int SeatID { get; set; }
		public required int RowNo { get; set; }
		public required int SeatNo { get; set; }
		public required SeatType SeatType { get; set; }
		public int AuditoriumID { get; set; }
	}
}
