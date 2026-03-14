using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaAPI.Core.Models.Enum;

namespace CinemaAPI.Core.Models
{
	public class TicketType
	{
		public required TicketTypeEnum Type { get; set; }
		public float? CurrentPrice { get; set; }
		public string? Description { get; set; }
	}
}
