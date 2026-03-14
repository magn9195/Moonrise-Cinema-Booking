using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaAPI.Core.Models
{
    public class Auditorium
    {
        public int AuditoriumID { get; set; }
        public required string Name { get; set; }
		public required int RowNum { get; set; }
        public required int SeatsPerRow { get; set; }
        public Cinema? Cinema { get; set; }
        public int CinemaID { get; set; }
	}
}
