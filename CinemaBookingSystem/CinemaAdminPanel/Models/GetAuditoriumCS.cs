using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaAdminPanel.Models
{
    public class GetAuditoriumCS
    {
        public int AuditoriumID { get; set; }
		public required string Name { get; set; }
		public required int RowNum { get; set; }
        public required int SeatsPerRow { get; set; }
        public int CinemaID { get; set; }
	}
}
