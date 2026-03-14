using CinemaAPI.Core.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaAPI.Core.Models
{
	public class Showtime
	{
		public int ShowtimeID { get; set; }
		public required ShowTypeEnum ShowType { get; set; }
		public required DateTime StartTime { get; set; }
		public int MovieID { get; set; }
		public int AuditoriumID { get; set; }
		public Movie? Movie { get; set; } = null;
		public Auditorium? Auditorium { get; set; } = null;
		public List<ShowtimeSeat> SeatAvailability { get; set; } = [];
	}
}
