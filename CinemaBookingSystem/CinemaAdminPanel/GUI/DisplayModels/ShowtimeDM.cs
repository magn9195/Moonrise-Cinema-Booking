using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaAdminPanel.GUI.DisplayModels
{
	public class ShowtimeDM
	{
		public int ShowtimeID { get; set; }
		public DateTime Date { get; set; }
		public required string StartTime { get; set; }
		public required string EndTime { get; set; }
		public required string MovieTitle { get; set; }
		public required string Duration { get; set; }
		public required string ShowtimeType { get; set; }
		public required string GapAfter { get; set; }
	}
}
