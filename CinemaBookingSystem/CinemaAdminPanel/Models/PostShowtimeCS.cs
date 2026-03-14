using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaAdminPanel.Models
{
	public class PostShowtimeCS
	{
		public required string ShowType { get; set; }
		public required DateTime StartTime { get; set; }
		public required int MovieID { get; set; }
		public required int AuditoriumID { get; set; }
	}
}
