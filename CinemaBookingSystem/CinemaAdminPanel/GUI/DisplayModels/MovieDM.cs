using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaAdminPanel.GUI.DisplayModels
{
	public class MovieDM
	{
		public int MovieID { get; set; }
		public required string Title { get; set; }
		public required string FormattedReleaseDate { get; set; }
		public required string DurationDisplay { get; set; }
		public required string GenreList { get; set; }
	}
}
