using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaAPI.Core.Models
{
    public class Cinema
    {
        public int CinemaID { get; set; }
        public required string Name { get; set; }
        public required Address Address { get; set; }
        public required List<Auditorium> Auditoriums { get; set; } = [];
	}
}
