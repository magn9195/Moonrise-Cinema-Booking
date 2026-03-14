using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaWebService.Models.Enum;

namespace CinemaWebService.Models
{
	public class GetSeatCS
	{
		public int SeatID { get; set; }
		public required int RowNo { get; set; }
		public required int SeatNo { get; set; }
		public required SeatTypeEnum SeatType { get; set; }
	}
}
